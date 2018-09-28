use iron::prelude::*;
use iron::status;

/// Check if the server is accepting requests
pub fn pong(_: &mut Request) -> IronResult<Response> {
    Ok(Response::with((status::Ok, "{\"pong\": true}")))
}
