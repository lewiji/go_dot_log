# Contributing

Thanks for helping! 

This project depends on [GoDotTest], but because [GoDotTest] depends on this project, we include [GoDotTest] locally as a project reference instead of a package reference on nuget.

When working with these projects, make sure you have clones of [GoDotLog] and [GoDotCollections] alongside [GoDotTest], as shown in the following:

```
- your projects directory/
  - go_dot_log/
    - ...
  - go_dot_collections/
    - ...
  - go_dot_test/
    - ...
```

Be sure to run `dotnet restore` in each project. Sometimes the projects can have trouble building if `dotnet restore` isn't run in the correct order, so you may have to play with the order to get it all working. Each project will also probably have to be opened in Godot and built at least once to generate the `.godot` build folders for in each project for C# to compile.

[GoDotTest]: https://github.com/chickensoft-games/go_dot_test
[GoDotLog]: https://github.com/chickensoft-games/go_dot_log
[GoDotCollections]: https://github.com/chickensoft-games/go_dot_collections
