# CVRLuaTools
<img align="right" src="https://github.com/user-attachments/assets/7939a0ee-d1b9-4872-82bb-c52e7640e5e4">

Simple utility script to aid in advanced ChilloutVR lua script creation.

Allows merging scripts, exposing variables in editor gui, and easily creating tables of objects via drag and drop.

### How to use:

- Use the `NAKLuaClientBehaviourWrapper` component instead of `CVRLuaClientBehaviour`
- On upload, the wrapper will automatically be replaced with a `CVRLuaClientBehaviour` referencing the generated script & BoundObjects. 
- All BoundEntries are available via the `BoundEntries` table, which is appended to the top of the script underneath the requires. 

### Credit:

Exterrata - Lua Script Merger utility <3

---

Here is the block of text where I tell you it's not my fault if you're bad at Unity.

> Use of this Unity Script is done so at the user's own risk and the creator cannot be held responsible for any issues arising from its use.
