using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Bankfeeds;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace XeroNetStandardApp.Controllers
{
  public class BankfeedConnections : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;

    public BankfeedConnections(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
    }

    // GET: /BankfeedConnections/
    [HttpGet]
    public async Task<ActionResult> Index()
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      var BankFeedsApi = new BankFeedsApi();

      var response = await BankFeedsApi.GetFeedConnectionsAsync(accessToken, xeroTenantId);
      
      ViewBag.jsonResponse = response.ToJson();

      return View(response.Items);
    }

    // GET: /BankfeedConnections#Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // POST: /BankfeedConnections#Create
    [HttpPost]
    public async Task<ActionResult> Create(
      string accountToken,
      string accountNumber,
      string accountType,
      string accountName,
      string currency,
      string country
    )
    {
      
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      var BankfeedsApi = new BankFeedsApi();

      Enum.TryParse<FeedConnection.AccountTypeEnum>(accountType, out var accountTypeEnum);
      Enum.TryParse<CurrencyCode>(currency, out var currencyCode);
      Enum.TryParse<CountryCode>(country, out var countryCode);

      var feedConnection = new FeedConnection{
        AccountToken = accountToken, 
        AccountNumber = accountNumber,
        AccountType =  accountTypeEnum,
        AccountName = accountName,
        Currency = currencyCode,
        Country = countryCode
      };

      List<FeedConnection> list = new List<FeedConnection>();
      list.Add(feedConnection);

      FeedConnections items = new FeedConnections{
        Pagination = new Pagination(),
        Items = list
      };

      await BankfeedsApi.CreateFeedConnectionsAsync(accessToken, xeroTenantId, items);

      return RedirectToAction("Index", "BankfeedConnections");
    }


    // Get: /Bankfeeds#Delete
    [HttpGet]
    public async Task<ActionResult> Delete(string bankfeedConnectionId)
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }
      
      Guid bankfeedConnectionIdGuid = Guid.Parse(bankfeedConnectionId);

      List<FeedConnection> list = new List<FeedConnection>();
      list.Add(
        new FeedConnection {
          Id = bankfeedConnectionIdGuid
        }
      );

      var feedConnections = new FeedConnections{
          Items = list
      };


      var BankFeedsApi = new BankFeedsApi();
      await BankFeedsApi.DeleteFeedConnectionsAsync(accessToken, xeroTenantId, feedConnections);
      
      return RedirectToAction("Index", "BankfeedConnections");
    }

  }
}