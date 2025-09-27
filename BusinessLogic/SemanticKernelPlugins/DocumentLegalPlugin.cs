using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace PalmHilsSemanticKernelBot.BusinessLogic.SemanticKernelPlugins
{
    public class DocumentLegalPlugin
    {
        [KernelFunction]
        [Description("get the legel docs needed to own a property")]
        public async Task<string> GetLegalDocs()
        {



            return $"birth certificate ,a vaild ID and a bank statment  ";
        }


    }
}
