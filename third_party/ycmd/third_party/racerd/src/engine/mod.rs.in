use std::path::Path;

/// This module's Error and Result types
mod error;
pub use self::error::{Error, Result};

use ::Config;

/// Provide completions, definitions, and analysis of rust source code
pub trait SemanticEngine : Send + Sync {
    /// Perform any necessary initialization.
    ///
    /// Only needs to be called once when an engine is created.
    fn initialize(&self, config: &Config) -> Result<()>;

    /// Find the definition for the item under the cursor
    fn find_definition(&self, context: &Context) -> Result<Option<Definition>>;

    /// Get a list of completions for the item under the cursor
    fn list_completions(&self, context: &Context) -> Result<Option<Vec<Completion>>>;
}

/// A possible completion for a location
#[derive(Debug)]
pub struct Completion {
    pub text: String,
    pub context: String,
    pub kind: String,
    pub file_path: String,
    pub position: CursorPosition,
}

/// Source file and type information for a found definition
#[derive(Debug)]
pub struct Definition {
    pub position: CursorPosition,
    pub text: String,
    pub text_context: String,
    pub dtype: String,
    pub file_path: String,
    pub docs: String,
}

/// Context for a given operation.
///
/// All operations require a buffer holding the contents of a file, the file's absolute path, and a
/// cursor position to fully specify the request. This object holds all of those items.
#[derive(Debug)]
pub struct Context {
    pub buffers: Vec<Buffer>,
    pub query_cursor: CursorPosition,
    pub query_file: String,
}

impl Context {
    pub fn new<T>(buffers: Vec<Buffer>, position: CursorPosition,
                  file_path: T) -> Context where T: Into<String> {
        Context {
            buffers: buffers,
            query_cursor: position,
            query_file: file_path.into(),
        }
    }

    pub fn query_path<'a>(&'a self) -> &'a Path {
        &Path::new(&self.query_file[..])
    }
}

/// Position of the cursor in a text file
///
/// Similar to a point, it has two coordinates `line` and `col`.
#[derive(Debug, Copy, Clone)]
pub struct CursorPosition {
    pub line: usize,
    pub col: usize,
}

impl Into<::racer::Location> for CursorPosition {
    fn into(self) -> ::racer::Location {
        ::racer::Location::Coords(::racer::Coordinate {
            line: self.line,
            column: self.col,
        })
    }
}

pub mod racer;
pub use self::racer::Racer;

#[derive(Debug, Deserialize, Clone)]
pub struct Buffer {
    pub file_path: String,
    pub contents: String,
}

impl Buffer {
    pub fn path<'a>(&'a self) -> &'a Path {
        &Path::new(&self.file_path[..])
    }
}
