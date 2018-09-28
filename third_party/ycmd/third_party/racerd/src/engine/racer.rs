//! SemanticEngine implementation for [the racer library](https://github.com/phildawes/racer)
//!
use engine::{SemanticEngine, Definition, Context, CursorPosition, Completion};

use racer::{self, Session, FileCache, Match, Coordinate};

use std::sync::Mutex;

use regex::Regex;

pub struct Racer {
    cache: Mutex<FileCache>
}

impl Racer {
    pub fn new() -> Racer {
        Racer {
            cache: Mutex::new(FileCache::default()),
        }
    }
}

// Racer's FileCache uses Rc and RefCell internally. We wrap everything in a
// Mutex, and the Rcs aren't possible to leak, so these should be ok. Correct
// solution is to make racer threadsafe.
unsafe impl Sync for Racer {}
unsafe impl Send for Racer {}

use ::Config;
use super::Result;

impl SemanticEngine for Racer {
    fn initialize(&self, config: &Config) -> Result<()> {
        if let Some(ref src_path) = config.rust_src_path {
            ::std::env::set_var("RUST_SRC_PATH", src_path);
        }

        Ok(())
    }

    fn find_definition(&self, ctx: &Context) -> Result<Option<Definition>> {
        let cache = match self.cache.lock() {
            Ok(guard) => guard,
            Err(poisoned) => poisoned.into_inner(),
        };

        let session = Session::new(&cache);
        for buffer in &ctx.buffers {
            session.cache_file_contents(buffer.path(), &*buffer.contents)
        }

        // TODO catch_panic: apparently this can panic! in a string operation. Something about pos
        // not landing on a character boundary.
        Ok(racer::find_definition(ctx.query_path(), ctx.query_cursor, &session)
            .and_then(|m| {
                m.coords.map(|Coordinate { line, column: col }| {
                    Definition {
                        position: CursorPosition {
                            line: line,
                            col: col
                        },
                        dtype: format!("{:?}", m.mtype),
                        file_path: m.filepath.to_str().unwrap().to_string(),
                        text: m.matchstr.clone(),
                        text_context: m.contextstr.clone(),
                        docs: m.docs.clone(),
                    }
                })
            }))
    }

    fn list_completions(&self, ctx: &Context) -> Result<Option<Vec<Completion>>> {
        let cache = match self.cache.lock() {
            Ok(guard) => guard,
            Err(poisoned) => poisoned.into_inner()
        };

        let session = Session::new(&cache);
        for buffer in &ctx.buffers {
            session.cache_file_contents(buffer.path(), &*buffer.contents)
        }

        let completions = racer::complete_from_file(ctx.query_path(), ctx.query_cursor, &session)
            .filter_map(|Match { matchstr, contextstr, mtype, filepath, coords, .. }| {
                coords.map(|Coordinate { line, column: col }| {
                    Completion {
                        position: CursorPosition {
                            line: line,
                            col: col
                        },
                        text: matchstr,
                        context: collapse_whitespace(&contextstr),
                        kind: format!("{:?}", mtype),
                        file_path: filepath.to_str().unwrap().to_string()
                    }
                })
            })
            .collect::<Vec<_>>();

        if completions.len() != 0 {
            Ok(Some(completions))
        } else {
            Ok(None)
        }
    }
}

pub fn collapse_whitespace(text: &str) -> String {
  Regex::new(r"\s+").unwrap().replace_all(text, " ")
}

#[cfg(test)]
mod tests {
    use super::*;
    use engine::{Context, CursorPosition, SemanticEngine, Buffer};

    use ::util::fs::TmpFile;

    #[test]
    #[allow(unused_variables)]
    fn find_definition() {
        // FIXME this is just here for side effects.
        let src2 = TmpFile::with_name("src2.rs", "
            /// myfn docs
            pub fn myfn() {}
            pub fn foo() {}
            ");

        let src = "
            use src2::*;
            mod src2;
            fn main() {
                myfn();
            }";

        let buffers = vec![Buffer {
            contents: src.to_string(),
            file_path: "src.rs".to_string()
        }];

        let ctx = Context::new(buffers, CursorPosition { line: 5, col: 17 }, "src.rs");
        let racer = Racer::new();
        let def = racer.find_definition(&ctx).unwrap().unwrap();

        assert_eq!(def.text, "myfn");
        assert_eq!(def.docs, "myfn docs");
    }

    #[test]
    #[allow(unused_variables)]
    fn find_completion() {
        let src = "
            mod mymod {
                /// myfn is a thing
                pub fn myfn<T>(reader: T) where T: ::std::io::Read {}
            }

            fn main() {
                mymod::my
            }";

        let buffers = vec![Buffer {
            contents: src.to_string(),
            file_path: "src.rs".to_string()
        }];

        let ctx = Context::new(buffers, CursorPosition { line: 8, col: 25 }, "src.rs");
        let racer = Racer::new();

        let completions = racer.list_completions(&ctx).unwrap().unwrap();
        assert_eq!(completions.len(), 1);

        let completion = &completions[0];
        assert_eq!(completion.text, "myfn");
        assert_eq!(completion.position.line, 4);
        assert_eq!(completion.file_path, "src.rs");
    }

    #[test]
    fn find_completion_collapsing_whitespace() {
        let src = "
            struct foo {}

            impl foo {
              fn format( &self )
                -> u32 {
              }
            }

            fn main() {
              let x = foo{};
              x.
            }";

        let buffers = vec![Buffer {
            contents: src.to_string(),
            file_path: "src.rs".to_string()
        }];

        let ctx = Context::new(buffers, CursorPosition { line: 12, col: 16 }, "src.rs");
        let racer = Racer::new();

        let completions = racer.list_completions(&ctx).unwrap().unwrap();
        assert_eq!(completions.len(), 1);

        let completion = &completions[0];
        assert_eq!(completion.context, "fn format( &self ) -> u32");
    }

    #[test]
    fn collapse_whitespace_test() {
        assert_eq!(collapse_whitespace("foo  foo"), "foo foo");
        assert_eq!(collapse_whitespace("  "), " ");
        assert_eq!(collapse_whitespace("\n\t  \n"), " ");
        assert_eq!(collapse_whitespace("foo\nbar"), "foo bar");
        assert_eq!(collapse_whitespace("fn foo( &self )\n   -> u32"),
                   "fn foo( &self ) -> u32");
    }
}
