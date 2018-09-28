racerd
======

JSON/HTTP Server based on [racer][] for adding Rust support to editors and IDEs

[![Build Status](https://travis-ci.org/jwilm/racerd.svg?branch=master)](https://travis-ci.org/jwilm/racerd)
[![Build status](https://ci.appveyor.com/api/projects/status/ysdf6dlxv73am0s5/branch/master?svg=true)](https://ci.appveyor.com/project/jwilm/racerd/branch/master)

![racerd_ycm](https://cloud.githubusercontent.com/assets/4285147/11676180/e924255e-9de4-11e5-9b32-5eda431f79a3.gif)

_YouCompleteMe in vim powered by racerd_


## Documentation

- [HTTP API](docs/API.md)
- [Rust Documentation](http://jwilm.github.io/racerd/libracerd/)
- [racerd options](https://github.com/jwilm/racerd/blob/master/src/bin/racerd.rs#L14)


## Features

- Find definition & list completions support via racer
- Searches rust standard library and dependency crates
- HMAC authentication
- Usable as both library and executable
- Library API offers direct calls to avoid HTTP overhead


[rust-openssl]: https://github.com/sfackler/rust-openssl
[rust-openssl's manual configuration instructions]: https://github.com/sfackler/rust-openssl#manual-configuration
[YouCompleteMe]: https://github.com/Valloric/YouCompleteMe
[racer]: https://github.com/phildawes/racer
[API.md]: docs/API.md
