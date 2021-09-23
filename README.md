# IMPORTANT NOTE
As I was developing this library for a game originally. Much of this code went straight into the other project, and so some things have been updated and nolonger work in this version, mainly the web-hosted data has all changed. We nolonger use Obj or Collada file formats, as we needed a lightweight option that had vertex colors, so we made our own file format type .epa, this is not updated in the code base, and so if running into errors due to Web Hosted Data, please revert to using the local store.
# CustomRaycast
This is a repo where I wrote some test code for a city builder that I am working on. As the name suggests, I started writing a custom raycast solution inside of unity, with my own custom bounding boxes. However, this test repo has grown to include:
- Custom Raycasting
- Struct based non-unity objects
- Custom bounding boxes
- OBJ reconstruction
- COLLADA (.dae) reconstruction
- Vertex-Color based shaders
- Lookup-textures for encoding data into shaders.

This repo has grown quite a lot since I started writing the custom raycast solution, and has lots of useful information in here. Feel free to use in here what you will. I have gotten all I need from it and will move any future developments into thier own respective private repositories.
