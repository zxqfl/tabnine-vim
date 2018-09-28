extern crate serde_codegen;

use std::env;
use std::fs;
use std::path::Path;

fn serde_expand(infile: &str, outfile: &str) {
    let out_dir = env::var_os("OUT_DIR").expect("OUT_DIR environment var is set");
    let src = Path::new(infile);
    let dst = Path::new(&out_dir).join(outfile);

    // Make output directory
    fs::create_dir(dst.parent().unwrap()).ok();

    serde_codegen::expand(&src, &dst).expect("codegen expand successfully");
}

pub fn main() {
    serde_expand("src/http/completion.rs.in", "http/completion.rs");
    serde_expand("src/http/definition.rs.in", "http/definition.rs");
    serde_expand("src/engine/mod.rs.in", "engine/mod.rs");
}

