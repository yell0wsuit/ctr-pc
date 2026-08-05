using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace CutTheRopeDX.Launcher.Graphics
{
    /// <summary>
    /// Outcome of checking the machine for a usable hardware Vulkan driver.
    /// </summary>
    public enum VulkanProbeResult
    {
        /// <summary>
        /// A Vulkan device that is not a CPU implementation, has a graphics queue, and comes with the
        /// required surface extensions was found.
        /// </summary>
        Hardware,

        /// <summary>The Vulkan loader is present but exposes no usable device.</summary>
        NoDevice,

        /// <summary>The Vulkan loader itself is missing or unusable.</summary>
        NoLoader,
    }

    /// <summary>
    /// Checks whether the machine has a usable hardware Vulkan driver.
    /// </summary>
    /// <remarks>
    /// Deliberately talks to <c>vulkan-1.dll</c> directly rather than through SDL or MonoGame, so that a
    /// failure here cannot leave the renderer's Vulkan loader in a half-initialised state. The library is
    /// unloaded before returning.
    /// </remarks>
    [SupportedOSPlatform("windows")]
    public static unsafe partial class VulkanProbe
    {
        private const uint VkSuccess = 0;
        private const uint VkStructureTypeApplicationInfo = 0;
        private const uint VkStructureTypeInstanceCreateInfo = 1;
        private const uint VkApiVersion10 = 1u << 22;
        private const uint VkQueueGraphicsBit = 0x1;

        /// <summary>Length of a Vulkan extension name buffer (VK_MAX_EXTENSION_NAME_SIZE).</summary>
        private const int MaxExtensionNameSize = 256;

        /// <summary>VK_PHYSICAL_DEVICE_TYPE_CPU: the device is a software rasteriser.</summary>
        private const uint VkPhysicalDeviceTypeCpu = 4;

        /// <summary>
        /// Byte offset of <c>deviceType</c> in <c>VkPhysicalDeviceProperties</c>, after the four uint32
        /// fields (apiVersion, driverVersion, vendorID, deviceID) that precede it.
        /// </summary>
        private const int PhysicalDeviceTypeOffset = 16;

        /// <summary>
        /// Buffer size handed to <c>vkGetPhysicalDeviceProperties</c>, comfortably above the real size of
        /// <c>VkPhysicalDeviceProperties</c> so the driver has room to write all of it.
        /// </summary>
        /// <remarks>
        /// The struct is roughly 800 bytes, most of it the embedded limits. Over-allocating on the stack
        /// costs nothing here and removes any need to track the exact layout across Vulkan versions, which
        /// only ever append.
        /// </remarks>
        private const int PhysicalDevicePropertiesSize = 2048;

        [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr LoadLibrary(string fileName);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool FreeLibrary(IntPtr module);

        // Takes byte* rather than string: GetProcAddress is ANSI-only, and passing a UTF-8 literal
        // keeps this consistent with the Vulkan entry-point lookups below and needs no marshaller.
        [LibraryImport("kernel32.dll")]
        private static partial IntPtr GetProcAddress(IntPtr module, byte* procName);

        [StructLayout(LayoutKind.Sequential)]
        private struct VkApplicationInfo
        {
            public uint SType;
            public IntPtr PNext;
            public IntPtr PApplicationName;
            public uint ApplicationVersion;
            public IntPtr PEngineName;
            public uint EngineVersion;
            public uint ApiVersion;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VkInstanceCreateInfo
        {
            public uint SType;
            public IntPtr PNext;
            public uint Flags;
            public IntPtr PApplicationInfo;
            public uint EnabledLayerCount;
            public IntPtr PpEnabledLayerNames;
            public uint EnabledExtensionCount;
            public IntPtr PpEnabledExtensionNames;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VkExtensionProperties
        {
            /// <summary>Null-terminated ASCII name, sized by VK_MAX_EXTENSION_NAME_SIZE.</summary>
            public fixed byte ExtensionName[MaxExtensionNameSize];

            public uint SpecVersion;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VkQueueFamilyProperties
        {
            public uint QueueFlags;
            public uint QueueCount;
            public uint TimestampValidBits;
            public uint MinImageTransferGranularityWidth;
            public uint MinImageTransferGranularityHeight;
            public uint MinImageTransferGranularityDepth;
        }

        /// <summary>
        /// Probes the machine for a usable hardware Vulkan device.
        /// </summary>
        /// <returns>
        /// <see cref="VulkanProbeResult.Hardware"/> when a device with a graphics queue exists,
        /// <see cref="VulkanProbeResult.NoDevice"/> when the loader works but no such device is present,
        /// or <see cref="VulkanProbeResult.NoLoader"/> when <c>vulkan-1.dll</c> is missing or unusable.
        /// </returns>
        public static VulkanProbeResult Run()
        {
            IntPtr library = IntPtr.Zero;
            try
            {
                library = LoadLibrary("vulkan-1.dll");
                return library == IntPtr.Zero ? VulkanProbeResult.NoLoader : Probe(library);
            }
            catch (Exception ex)
            {
                // Covers the whole body so callers need no guard of their own. An unusable loader and one
                // that throws on the way in mean the same thing to everyone upstream.
                Console.Error.WriteLine($"[graphics] Vulkan probe failed: {ex.Message}");
                return VulkanProbeResult.NoDevice;
            }
            finally
            {
                if (library != IntPtr.Zero)
                {
                    _ = FreeLibrary(library);
                }
            }
        }

        private static VulkanProbeResult Probe(IntPtr library)
        {
            IntPtr getInstanceProcAddrPtr;
            fixed (byte* name = "vkGetInstanceProcAddr"u8)
            {
                getInstanceProcAddrPtr = GetProcAddress(library, name);
            }

            if (getInstanceProcAddrPtr == IntPtr.Zero)
            {
                return VulkanProbeResult.NoLoader;
            }

            delegate* unmanaged[Stdcall]<IntPtr, byte*, IntPtr> getInstanceProcAddr =
                (delegate* unmanaged[Stdcall]<IntPtr, byte*, IntPtr>)getInstanceProcAddrPtr;

            if (!HasRequiredSurfaceExtensions(getInstanceProcAddr))
            {
                return VulkanProbeResult.NoDevice;
            }

            IntPtr createInstancePtr = LoadGlobalFunction(getInstanceProcAddr, "vkCreateInstance"u8);
            if (createInstancePtr == IntPtr.Zero)
            {
                return VulkanProbeResult.NoLoader;
            }

            VkApplicationInfo appInfo = new()
            {
                SType = VkStructureTypeApplicationInfo,
                ApiVersion = VkApiVersion10,
            };

            VkInstanceCreateInfo createInfo = new()
            {
                SType = VkStructureTypeInstanceCreateInfo,
                PApplicationInfo = (IntPtr)(&appInfo),
            };

            delegate* unmanaged[Stdcall]<VkInstanceCreateInfo*, IntPtr, IntPtr*, uint> createInstance =
                (delegate* unmanaged[Stdcall]<VkInstanceCreateInfo*, IntPtr, IntPtr*, uint>)createInstancePtr;

            IntPtr instance;
            if (createInstance(&createInfo, IntPtr.Zero, &instance) != VkSuccess || instance == IntPtr.Zero)
            {
                return VulkanProbeResult.NoDevice;
            }

            try
            {
                return ProbeDevices(getInstanceProcAddr, instance);
            }
            finally
            {
                IntPtr destroyInstancePtr = LoadInstanceFunction(getInstanceProcAddr, instance, "vkDestroyInstance"u8);
                if (destroyInstancePtr != IntPtr.Zero)
                {
                    ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr, void>)destroyInstancePtr)(instance, IntPtr.Zero);
                }
            }
        }

        private static VulkanProbeResult ProbeDevices(
            delegate* unmanaged[Stdcall]<IntPtr, byte*, IntPtr> getInstanceProcAddr,
            IntPtr instance)
        {
            IntPtr enumeratePtr = LoadInstanceFunction(getInstanceProcAddr, instance, "vkEnumeratePhysicalDevices"u8);
            IntPtr queuePropsPtr = LoadInstanceFunction(getInstanceProcAddr, instance, "vkGetPhysicalDeviceQueueFamilyProperties"u8);
            IntPtr devicePropsPtr = LoadInstanceFunction(getInstanceProcAddr, instance, "vkGetPhysicalDeviceProperties"u8);
            if (enumeratePtr == IntPtr.Zero || queuePropsPtr == IntPtr.Zero || devicePropsPtr == IntPtr.Zero)
            {
                return VulkanProbeResult.NoDevice;
            }

            delegate* unmanaged[Stdcall]<IntPtr, uint*, IntPtr*, uint> enumerate =
                (delegate* unmanaged[Stdcall]<IntPtr, uint*, IntPtr*, uint>)enumeratePtr;
            delegate* unmanaged[Stdcall]<IntPtr, uint*, VkQueueFamilyProperties*, void> queueProps =
                (delegate* unmanaged[Stdcall]<IntPtr, uint*, VkQueueFamilyProperties*, void>)queuePropsPtr;
            delegate* unmanaged[Stdcall]<IntPtr, byte*, void> deviceProps =
                (delegate* unmanaged[Stdcall]<IntPtr, byte*, void>)devicePropsPtr;

            uint deviceCount = 0;
            if (enumerate(instance, &deviceCount, null) != VkSuccess || deviceCount == 0)
            {
                return VulkanProbeResult.NoDevice;
            }

            IntPtr[] devices = new IntPtr[deviceCount];
            fixed (IntPtr* devicePtr = devices)
            {
                if (enumerate(instance, &deviceCount, devicePtr) != VkSuccess)
                {
                    return VulkanProbeResult.NoDevice;
                }
            }

            // A device that cannot present is no use to us, so require a graphics-capable queue family,
            // and a device that is not really a GPU is worse than no Vulkan at all: rendering would fall
            // to a CPU implementation while the launcher reported hardware and skipped the OpenGL build.
            for (uint i = 0; i < deviceCount; i++)
            {
                if (IsCpuDevice(deviceProps, devices[i]))
                {
                    continue;
                }

                uint familyCount = 0;
                queueProps(devices[i], &familyCount, null);
                if (familyCount == 0)
                {
                    continue;
                }

                VkQueueFamilyProperties[] families = new VkQueueFamilyProperties[familyCount];
                fixed (VkQueueFamilyProperties* familyPtr = families)
                {
                    queueProps(devices[i], &familyCount, familyPtr);
                }

                for (uint f = 0; f < familyCount; f++)
                {
                    if ((families[f].QueueFlags & VkQueueGraphicsBit) != 0)
                    {
                        return VulkanProbeResult.Hardware;
                    }
                }
            }

            return VulkanProbeResult.NoDevice;
        }

        /// <summary>
        /// Whether a physical device reports itself as a CPU implementation rather than a GPU.
        /// </summary>
        /// <param name="deviceProps">Resolved <c>vkGetPhysicalDeviceProperties</c>.</param>
        /// <param name="device">Physical device to query.</param>
        /// <returns><see langword="true" /> when the device is a software rasteriser.</returns>
        /// <remarks>
        /// Only <c>deviceType</c> is wanted, and it sits at a fixed offset near the front of
        /// <c>VkPhysicalDeviceProperties</c>, ahead of the large embedded limits and sparse-properties
        /// structures. Declaring those in full to reach a field 16 bytes in would be a great deal of
        /// layout to keep correct for no gain, so the driver is given a buffer comfortably larger than
        /// the struct and only that field is read back.
        /// <para>
        /// Types other than CPU are all accepted, including <c>OTHER</c> and <c>VIRTUAL_GPU</c>: a
        /// paravirtualised GPU is still hardware doing the work, and a driver that declines to classify
        /// itself is not evidence of software rendering. Only the unambiguous case is rejected.
        /// </para>
        /// </remarks>
        private static bool IsCpuDevice(
            delegate* unmanaged[Stdcall]<IntPtr, byte*, void> deviceProps,
            IntPtr device)
        {
            byte* properties = stackalloc byte[PhysicalDevicePropertiesSize];
            new Span<byte>(properties, PhysicalDevicePropertiesSize).Clear();
            deviceProps(device, properties);
            return *(uint*)(properties + PhysicalDeviceTypeOffset) == VkPhysicalDeviceTypeCpu;
        }

        /// <summary>
        /// Confirms the loader exposes the surface extensions MonoGame's Vulkan backend requires.
        /// </summary>
        /// <remarks>
        /// MonoGame aborts with "Installed Vulkan doesn't implement the VK_KHR_win32_surface extension"
        /// when these are absent, so a loader without them is no more use to us than no loader at all.
        /// </remarks>
        private static bool HasRequiredSurfaceExtensions(
            delegate* unmanaged[Stdcall]<IntPtr, byte*, IntPtr> getInstanceProcAddr)
        {
            IntPtr enumeratePtr = LoadGlobalFunction(getInstanceProcAddr, "vkEnumerateInstanceExtensionProperties"u8);
            if (enumeratePtr == IntPtr.Zero)
            {
                return false;
            }

            delegate* unmanaged[Stdcall]<byte*, uint*, VkExtensionProperties*, uint> enumerate =
                (delegate* unmanaged[Stdcall]<byte*, uint*, VkExtensionProperties*, uint>)enumeratePtr;

            uint count = 0;
            if (enumerate(null, &count, null) != VkSuccess || count == 0)
            {
                return false;
            }

            bool hasSurface = false;
            bool hasWin32Surface = false;

            VkExtensionProperties[] extensions = new VkExtensionProperties[count];
            fixed (VkExtensionProperties* extensionPtr = extensions)
            {
                if (enumerate(null, &count, extensionPtr) != VkSuccess)
                {
                    return false;
                }

                for (uint i = 0; i < count; i++)
                {
                    string name = ReadExtensionName(extensionPtr[i].ExtensionName);
                    if (string.Equals(name, "VK_KHR_surface", StringComparison.Ordinal))
                    {
                        hasSurface = true;
                    }
                    else if (string.Equals(name, "VK_KHR_win32_surface", StringComparison.Ordinal))
                    {
                        hasWin32Surface = true;
                    }
                }
            }

            return hasSurface && hasWin32Surface;
        }

        private static string ReadExtensionName(byte* name)
        {
            ReadOnlySpan<byte> raw = new(name, MaxExtensionNameSize);
            int length = raw.IndexOf((byte)0);
            return Encoding.UTF8.GetString(length < 0 ? raw : raw[..length]);
        }

        private static IntPtr LoadGlobalFunction(
            delegate* unmanaged[Stdcall]<IntPtr, byte*, IntPtr> getInstanceProcAddr,
            ReadOnlySpan<byte> nameUtf8)
        {
            fixed (byte* name = nameUtf8)
            {
                return getInstanceProcAddr(IntPtr.Zero, name);
            }
        }

        private static IntPtr LoadInstanceFunction(
            delegate* unmanaged[Stdcall]<IntPtr, byte*, IntPtr> getInstanceProcAddr,
            IntPtr instance,
            ReadOnlySpan<byte> nameUtf8)
        {
            fixed (byte* name = nameUtf8)
            {
                return getInstanceProcAddr(instance, name);
            }
        }
    }
}
