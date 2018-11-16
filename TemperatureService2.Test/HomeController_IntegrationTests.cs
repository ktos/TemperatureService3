﻿using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TemperatureService2.Models;
using TemperatureService2.Test.Helpers;
using Xunit;

namespace TemperatureService2.Test
{
    public class HomeController_Integration : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public HomeController_Integration(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Home/Index")]
        [InlineData("/outdoor.html")]
        [InlineData("/outdoor")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url).ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("/nonexisting.html")]
        [InlineData("/nonexisting.json")]
        [InlineData("/nonexisting.wns")]
        public async Task Get_SensorsReturn404WhenNonExisting(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("/outdoor.json")]
        [InlineData("/indoor.json")]
        public async Task Get_SensorReturn200AndCorrectContentType(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url).ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("/outdoor.wns")]
        [InlineData("/indoor.wns")]
        public async Task Get_SensorReturn200AndCorrectContentTypeAndCorrectWNSExpires(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url).ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(await response.Content.ReadAsStringAsync());

            var text = xd.SelectNodes("/tile/visual/binding[@template=\"TileWide\"]/text[@hint-style=\"captionSubtle\"]").Item(0).InnerText;
            text = text.Replace("last update: ", "");

            var lastUpdated = DateTime.Parse(text);
            var wnsExpires = DateTime.Parse(response.Headers.GetValues("X-WNS-Expires").First()).ToUniversalTime();

            Assert.Equal(lastUpdated.AddMinutes(60), wnsExpires);
            Assert.Equal("application/xml; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("outdoor")]
        [InlineData("indoor")]
        public async Task Put_SensorDataApiKeyInBody(string sensor)
        {
            // Arrange
            var client = _factory.CreateClient();
            var url = $"/{sensor}";

            var dto = new SensorDto { ApiKey = "testapi", Data = 9 };
            var content = JsonConvert.SerializeObject(dto);

            // Act
            var response = await client.PutAsync(url, new StringContent(content, Encoding.UTF8, "application/json"))
                .ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299

            var responseBody = await response.Content.ReadAsStringAsync();
            var svm = JsonConvert.DeserializeObject<SensorDto>(responseBody);

            Assert.Equal(9.0f, svm.Data);

            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("outdoor")]
        [InlineData("indoor")]
        public async Task Put_SensorDataApiKeyInHeaders(string sensor)
        {
            // Arrange
            var client = _factory.CreateClient();
            var url = $"/{sensor}";

            var dto = new SensorDto { Data = 9 };
            var content = JsonConvert.SerializeObject(dto);

            client.DefaultRequestHeaders.Add("X-APIKEY", "testapi");

            // Act
            var response = await client.PutAsync(url, new StringContent(content, Encoding.UTF8, "application/json"))
                .ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299

            var responseBody = await response.Content.ReadAsStringAsync();
            var svm = JsonConvert.DeserializeObject<SensorDto>(responseBody);

            Assert.Equal(9.0f, svm.Data);

            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("outdoor")]
        [InlineData("indoor")]
        public async Task Put_SensorInformationApiKeyInBody(string sensor)
        {
            // Arrange
            var client = _factory.CreateClient();
            var url = $"/{sensor}";

            string randomId = Utilities.RandomString(5);
            string randomDescription = Utilities.RandomString(20);

            var dto = new SensorDto { Id = randomId, Description = randomDescription, ApiKey = "testapi" };
            var content = JsonConvert.SerializeObject(dto);

            // Act
            var response = await client.PutAsync(url, new StringContent(content, Encoding.UTF8, "application/json"))
                .ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299

            var responseBody = await response.Content.ReadAsStringAsync();
            var svm = JsonConvert.DeserializeObject<SensorDto>(responseBody);

            Assert.Equal(randomId, svm.Id);
            Assert.Equal(randomDescription, svm.Description);

            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("outdoor")]
        [InlineData("indoor")]
        public async Task Put_SensorInformationApiKeyInHeaders(string sensor)
        {
            // Arrange
            var client = _factory.CreateClient();
            var url = $"/{sensor}";

            string randomId = Utilities.RandomString(5);
            string randomDescription = Utilities.RandomString(20);

            var dto = new SensorDto { Id = randomId, Description = randomDescription };
            var content = JsonConvert.SerializeObject(dto);

            client.DefaultRequestHeaders.Add("X-APIKEY", "testapi");

            // Act
            var response = await client.PutAsync(url, new StringContent(content, Encoding.UTF8, "application/json"))
                .ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299

            var responseBody = await response.Content.ReadAsStringAsync();
            var svm = JsonConvert.DeserializeObject<SensorDto>(responseBody);

            Assert.Equal(randomId, svm.Id);
            Assert.Equal(randomDescription, svm.Description);

            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("outdoor")]
        [InlineData("indoor")]
        public async Task Put_SensorInformationWrongApiKeyInBody(string sensor)
        {
            // Arrange
            var client = _factory.CreateClient();
            var url = $"/{sensor}";

            string randomId = Utilities.RandomString(5);
            string randomDescription = Utilities.RandomString(20);

            var dto = new SensorDto { Id = randomId, Description = randomDescription, ApiKey = "wrongapikey" };
            var content = JsonConvert.SerializeObject(dto);

            // Act
            var response = await client.PutAsync(url, new StringContent(content, Encoding.UTF8, "application/json"))
                .ConfigureAwait(false);

            // Assert
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("outdoor")]
        [InlineData("indoor")]
        public async Task Put_SensorInformationWrongApiKeyInHeaders(string sensor)
        {
            // Arrange
            var client = _factory.CreateClient();
            var url = $"/{sensor}";

            string randomId = Utilities.RandomString(5);
            string randomDescription = Utilities.RandomString(20);

            var dto = new SensorDto { Id = randomId, Description = randomDescription };
            var content = JsonConvert.SerializeObject(dto);

            client.DefaultRequestHeaders.Add("X-APIKEY", "wrongapikey");

            // Act
            var response = await client.PutAsync(url, new StringContent(content, Encoding.UTF8, "application/json"))
                .ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("outdoor")]
        [InlineData("indoor")]
        public async Task Put_SensorDataLikeDevice(string sensor)
        {
            // Arrange
            var client = _factory.CreateClient();
            var url = $"/{sensor}";

            var content = "{ \"name\": \"" + sensor + "\", \"data\": \"" + "9.0" + "\", \"apikey\": \"" + "testapi" + "\" }";

            // Act
            var response = await client.PutAsync(url, new StringContent(content, Encoding.UTF8, "application/json"))
                .ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299

            var responseBody = await response.Content.ReadAsStringAsync();
            var svm = JsonConvert.DeserializeObject<SensorDto>(responseBody);

            Assert.Equal(9.0f, svm.Data);

            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }
    }
}