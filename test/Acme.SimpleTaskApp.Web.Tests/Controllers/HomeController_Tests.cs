using System.Threading.Tasks;
using Acme.SimpleTaskApp.Models.TokenAuth;
using Acme.SimpleTaskApp.Web.Controllers;
using Shouldly;
using Xunit;

namespace Acme.SimpleTaskApp.Web.Tests.Controllers
{
    public class HomeController_Tests: SimpleTaskAppWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.Index))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}