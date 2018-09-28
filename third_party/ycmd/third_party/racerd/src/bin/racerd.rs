extern crate docopt;
extern crate rustc_serialize;
extern crate libracerd;
extern crate env_logger;

use libracerd::{Config, engine, http};
use libracerd::engine::SemanticEngine;

use std::convert::Into;

use docopt::Docopt;

const VERSION: &'static str = env!("CARGO_PKG_VERSION");
const USAGE: &'static str = "
racerd - a JSON/HTTP layer on top of racer

Usage:
  racerd serve [--secret-file=<path>] [--port=<int>] [--addr=<address>] [-l] [--rust-src-path=<path>]
  racerd (-h | --help)
  racerd --version

Options:
  -c, --rust-src-path=<path>  Use the given path for std library completions
  -l, --logging               Print http logs.
  -h, --help                  Show this message.
  -p, --port=<int>            Listen on this port [default: 3048].
  -a, --addr=<address>        Listen on this address [default: 127.0.0.1].
  -s, --secret-file=<path>    Path to the HMAC secret file. File will be destroyed after being read.
  --version                   Print the version and exit.
";

#[derive(Debug, RustcDecodable)]
struct Args {
    flag_port: u16,
    flag_addr: String,
    flag_version: bool,
    flag_secret_file: Option<String>,
    flag_logging: bool,
    flag_rust_src_path: Option<String>,
    cmd_serve: bool
}

impl Into<Config> for Args {
    fn into(self) -> Config {
        Config {
            port: self.flag_port as u16,
            secret_file: self.flag_secret_file,
            print_http_logs: self.flag_logging,
            rust_src_path: self.flag_rust_src_path,
            addr: self.flag_addr
        }
    }
}

fn main() {
    // Start logging
    ::env_logger::init().unwrap();

    // Parse arguments
    let args: Args = Docopt::new(USAGE)
                            .and_then(|d| d.decode())
                            .unwrap_or_else(|e| e.exit());

    // Print version and exit if --version was specified
    if args.flag_version {
        println!("racerd version {}", VERSION);
        ::std::process::exit(0);
    }

    // build config object
    let config: Config = args.into();

    // TODO start specified semantic engine. For now, hard coded racer.
    let racer = engine::Racer::new();
    racer.initialize(&config).unwrap();

    // Start serving
    let server = http::serve(&config, racer).unwrap();
    println!("racerd listening at {}", server.addr());
}
