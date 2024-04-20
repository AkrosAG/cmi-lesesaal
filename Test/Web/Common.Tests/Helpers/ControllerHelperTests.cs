using System.Security.Claims;
using System.Web.Http;
using CMI.Contract.Common;
using CMI.Web.Common.api;
using FluentAssertions;
using NUnit.Framework;

namespace CMI.Web.Common.Tests.Helpers;

[TestFixture]
public class ControllerHelperTests
{
    public class ApiControllerHelper : ApiController
    {
        public ApiControllerHelper(ClaimsPrincipal claimsPrincipal)
        {
            this.RequestContext.Principal = claimsPrincipal;
        }
    }

    [Test]
    public void Initial_role_should_be_oe2_for_home_organization_is_null()
    {
        // arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("homeOrganization", ""),
            new Claim("affiliation", "")
        }));
        var controllerFake = new ApiControllerHelper(principal);
        var controllerHelper = new ControllerHelper(controllerFake);

        // act
        var initialToken = controllerHelper.GetInitialTokenFromClaims();

        // assert
        initialToken.Should().Be(AccessRoles.RoleOe2);
    }


    [Test]
    public void Initial_role_should_be_oe2_for_home_organization_is_not_ethz_and_affiliation_is_not_staff_or_member()
    {
        // arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("homeOrganization", "basel.ch"),
            new Claim("affiliation", "praesident")
        }));
        var controllerFake = new ApiControllerHelper(principal);
        var controllerHelper = new ControllerHelper(controllerFake);

        // act
        var initialToken = controllerHelper.GetInitialTokenFromClaims();

        // assert
        initialToken.Should().Be(AccessRoles.RoleOe2);
    }

    [Test]
    public void Initial_role_should_be_oe3_for_any_home_organization_and_affiliation_is_member()
    {
        // arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("homeOrganization", "eduid.ch"),
            new Claim("affiliation", "member")
        }));
        var controllerFake = new ApiControllerHelper(principal);
        var controllerHelper = new ControllerHelper(controllerFake);

        // act
        var initialToken = controllerHelper.GetInitialTokenFromClaims();

        // assert
        initialToken.Should().Be(AccessRoles.RoleOe3);
    }

    [Test]
    public void Initial_role_should_be_oe3_for_any_home_organization_and_affiliation_is_staff()
    {
        // arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("homeOrganization", "unibs.ch"),
            new Claim("affiliation", "staff")
        }));
        var controllerFake = new ApiControllerHelper(principal);
        var controllerHelper = new ControllerHelper(controllerFake);

        // act
        var initialToken = controllerHelper.GetInitialTokenFromClaims();

        // assert
        initialToken.Should().Be(AccessRoles.RoleOe3);
    }

    [Test]
    public void Initial_role_should_be_oe3_for_home_organization_ethz_and_affiliation_is_member()
    {
        // arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("homeOrganization", "ETHz.ch"),
            new Claim("affiliation", "member")
        }));
        var controllerFake = new ApiControllerHelper(principal);
        var controllerHelper = new ControllerHelper(controllerFake);

        // act
        var initialToken = controllerHelper.GetInitialTokenFromClaims();

        // assert
        initialToken.Should().Be(AccessRoles.RoleOe3);
    }

    [Test]
    public void Initial_role_should_be_EMA_for_home_organization_ethz_and_affiliation_is_staff()
    {
        // arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("homeOrganization", "ethZ.ch"),
            new Claim("affiliation", "staff")
        }));
        var controllerFake = new ApiControllerHelper(principal);
        var controllerHelper = new ControllerHelper(controllerFake);

        // act
        var initialToken = controllerHelper.GetInitialTokenFromClaims();

        // assert
        initialToken.Should().Be(AccessRoles.RoleEMA);
    }

}