# Contributing

**Your contribution is welcome!** 🙂

## CI Configurations

**AppVeyor** builds the solution using Visual Studio 2017 in an Windows environment. All the commits including PRs are built there. It also runs the unit tests. [See AppVeyor file].

**Travis-CI** builds, tests, and deploys the project. It uses Docker containers and multi-stage builds.

[See AppVeyor file]: ./.appveyor.yml
[See Travis file]: ./.travis.yml