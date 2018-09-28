using Nancy;
using Nancy.ModelBinding;
using OmniSharp.Parser;

namespace OmniSharp.UpdateBuffer
{
	/// <summary>
	/// Used to update OmniSharp's view of a buffer
	/// </summary>
	public class UpdateBufferModule : NancyModule
    {
		public UpdateBufferModule(BufferParser bufferParser)
		{
			Post["UpdateBuffer", "/updatebuffer"] = x =>
        	{
				var req = this.Bind<Common.Request>();
				bufferParser.ParsedContent(req.Buffer, req.FileName);
				return Response.AsJson(true);
			};
        }
    }
}
