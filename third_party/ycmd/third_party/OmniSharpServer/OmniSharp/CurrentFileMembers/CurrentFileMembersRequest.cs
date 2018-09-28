using OmniSharp.Common;

namespace OmniSharp.CurrentFileMembers
{
    public class CurrentFileMembersRequest : Request
    {
        public bool ShowAccessModifiers { get; set; }

        public CurrentFileMembersRequest()
        {
            ShowAccessModifiers = true;
        }
    }
}
