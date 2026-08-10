using System.Reflection;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class MouseCarrySingleSourceTests
    {
        private const BindingFlags InstanceFields = BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic;

        [Fact]
        public void MouseOwnsOneCarryValueInsteadOfIndependentCandyFields()
        {
            FieldInfo[] fields = typeof(Mouse).GetFields(InstanceFields);

            FieldInfo carry = Assert.Single(fields, field => field.FieldType.Name == "MouseCarry");
            Assert.Equal("carry", carry.Name);
            Assert.DoesNotContain(fields, field => field.Name is "carriedStar" or "carriedCandy");
        }

        [Fact]
        public void ManagerAndCandyAttachmentsDoNotRetainMouseCarryState()
        {
            FieldInfo[] managerFields = typeof(MiceObject).GetFields(InstanceFields);

            Assert.DoesNotContain(managerFields, field => field.Name is "carriedStar" or "carriedCandy");
            Assert.DoesNotContain(managerFields, field => field.FieldType.Name == "MouseCarry");
            Assert.Null(typeof(CandyAttachments).GetProperty("CarriedByMouse"));
            Assert.Null(typeof(CandyAttachments).GetMethod("SetCarriedByMouse"));
        }
    }
}
