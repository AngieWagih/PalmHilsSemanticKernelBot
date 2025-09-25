namespace PalmHilsSemanticKernelBot.Controllers.Response
{
    public class DirectLineTokenResponse
    {
        public DirectLineTokenResponse(string token, int expires_in)
        {
            this.token = token;
            this.expires_in = expires_in;
        }

        public string token { get; set; }
        public int expires_in { get; set; }
    }
}