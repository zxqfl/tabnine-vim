use iron::prelude::*;
use iron::status;
use iron::mime::Mime;

use serde_json::to_string;

use engine::{Definition, Context, CursorPosition, Buffer};
use super::EngineProvider;

/// Given a location, return where the identifier is defined
///
/// Possible responses include
///
/// - `200 OK` the request was successful and a JSON object is returned.
/// - `204 No Content` the request was successful, but no match was found.
/// - `400 Bad Request` the request payload was malformed
/// - `500 Internal Server Error` some unexpected error occurred
pub fn find(req: &mut Request) -> IronResult<Response> {

    // Parse the request. If the request doesn't parse properly, the request is invalid, and a 400
    // BadRequest is returned.
    let fdr = match req.get::<::bodyparser::Struct<FindDefinitionRequest>>() {
        Ok(Some(s)) => {
            trace!("definition::find parsed FindDefinitionRequest");
            s
        },
        Ok(None) => {
            trace!("definition::find failed parsing FindDefinitionRequest");
            return Ok(Response::with(status::BadRequest))
        },
        Err(err) => {
            trace!("definition::find received error while parsing FindDefinitionRequest");
            return Err(IronError::new(err, status::InternalServerError))
        }
    };

    let mutex = req.get::<::persistent::Write<EngineProvider>>().unwrap();
    let engine = mutex.lock().unwrap_or_else(|e| e.into_inner());
    match engine.find_definition(&fdr.context()) {
        // 200 OK; found the definition
        Ok(Some(definition)) => {
            trace!("definition::find got a match");
            let res = FindDefinitionResponse::from(definition);
            let content_type = "application/json".parse::<Mime>().unwrap();
            Ok(Response::with((content_type, status::Ok, to_string(&res).unwrap())))
        },

        // 204 No Content; Everything went ok, but the definition was not found.
        Ok(None) => {
            trace!("definition::find did not find a match");
            Ok(Response::with(status::NoContent))
        },

        // 500 Internal Server Error; Error occurred while searching for the definition
        Err(err) => {
            trace!("definition::find encountered an error");
            Err(IronError::new(err, status::InternalServerError))
        }
    }
}

impl From<Definition> for FindDefinitionResponse {
    fn from(def: Definition) -> FindDefinitionResponse {
        FindDefinitionResponse {
            file_path: def.file_path,
            column: def.position.col,
            line: def.position.line,
            text: def.text,
            context: def.text_context,
            kind: def.dtype,
            docs: def.docs,
        }
    }
}

#[derive(Debug, Deserialize, Clone)]
struct FindDefinitionRequest {
    pub buffers: Vec<Buffer>,
    pub file_path: String,
    pub column: usize,
    pub line: usize,
}

impl FindDefinitionRequest {
    pub fn context(self) -> Context {
        let cursor = CursorPosition { line: self.line, col: self.column };
        Context::new(self.buffers, cursor, self.file_path)
    }
}

#[test]
fn find_definition_request_from_json() {
    let s = stringify!({
        "file_path": "src.rs",
        "buffers": [{
            "file_path": "src.rs",
            "contents": "fn foo() {}\nfn bar() {}\nfn main() {\nfoo();\n}"
        }],
        "line": 4,
        "column": 3
    });

    let req: FindDefinitionRequest = ::serde_json::from_str(s).unwrap();
    assert_eq!(req.file_path, "src.rs");
    assert_eq!(req.line, 4);
    assert_eq!(req.column, 3);
}

#[derive(Debug, Deserialize, Serialize)]
struct FindDefinitionResponse {
    pub file_path: String,
    pub column: usize,
    pub line: usize,
    pub text: String,
    pub context: String,
    pub kind: String,
    pub docs: String
}
