# How to make custom Rich Presence for your mod

1. Create your own application at <https://discord.com/developers/applications>
2. Name it as your mod and change the main picture. This will be the big app icon.
3. Copy its `Application ID` and set `DISCORD_APP_ID` in `RPCHelpers.cs` to that value.
4. Go to `Rich Presence` -> `Art Assets` and upload your pack images in `Rich Presence Assets`. Make sure the key (name) for each image is `pack_[number]` (starting from 1).
5. All done! Make sure all images are at least 512x512.

The original RPC images are provided in the `discord-rpc` folder as an example, in case you want to use it.
