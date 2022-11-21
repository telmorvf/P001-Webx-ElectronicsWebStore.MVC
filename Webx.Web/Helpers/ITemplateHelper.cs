using System.Threading.Tasks;

namespace Webx.Web.Helpers
{
    public interface ITemplateHelper
    {
        Task<string> RenderAsync<TViewModel>(string templateFileName,TViewModel viewModel);

    }
}
