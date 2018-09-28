//! libracerd provides an http server and set of completion engines for consumption by rust
//! developer tools. The http server itself is a few JSON endpoints providing completion, definition
//! look-up, and compilation. The endpoints are backed by an object implementing `SemanticEngine`
//!
//! Documentation for the HTTP endpoints can be found in the http module header.
//!
//! This project's source code is [available on GitHub](https://github.com/jwilm/racerd).
extern crate serde;
extern crate serde_json;

#[macro_use]
extern crate router;     // Iron routing handler
extern crate bodyparser; // Iron body parsing middleware
extern crate persistent; // Iron storage middleware
extern crate logger;     // Iron logging middleware
extern crate iron;       // http framework
extern crate iron_hmac;

extern crate hyper;      // Provides `Listening` type returned by Iron

#[macro_use]
extern crate log;        // log macros
extern crate racer;      // rust code analysis

extern crate rand;
extern crate regex;

pub mod util;
pub mod http;
pub mod engine;

use std::io::Read;
use std::fs::File;

/// Configuration flags and values
///
/// This object contains all switches the consumer has control of.
#[derive(Debug)]
pub struct Config {
    pub port: u16,
    pub addr: String,
    pub secret_file: Option<String>,
    pub print_http_logs: bool,
    pub rust_src_path: Option<String>
}

impl Default for Config {
    fn default() -> Config {
        Config {
            port: 0,
            addr: "127.0.0.1".to_owned(),
            secret_file: None,
            print_http_logs: false,
            rust_src_path: None,
        }
    }
}

impl Config {
    /// Build a default config object
    pub fn new() -> Config {
        Default::default()
    }

    /// Return contents of secret file
    ///
    /// panics if self.secret_file is None or an error is encountered while reading the file.
    pub fn read_secret_file(&self) -> Vec<u8> {
        self.secret_file.as_ref().map(|secret_file_path| {
            let buf = {
                let mut f = File::open(secret_file_path).unwrap();
                let mut buf = Vec::new();
                f.read_to_end(&mut buf).unwrap();
                buf
            };

            ::std::fs::remove_file(secret_file_path).unwrap();

            buf
        }).unwrap()
    }
}
