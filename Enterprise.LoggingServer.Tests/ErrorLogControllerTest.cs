﻿using Enterprise.Abstract.NetStandard;
using Enterprise.Comparer.NetStandard;
using Enterprise.Constants.NetStandard;
using Enterprise.Fixtures.NetStandard;
using Enterprise.LoggingServer.DataLayers.Mongo;
using Enterprise.LoggingServer.MockData;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Enterprise.LoggingServer.Tests
{
    [Collection("Logging Test Collection")]
    public class ErrorLogControllerTest : BaseTest
    {
        public ErrorLogControllerTest(AuthorizationServiceFixture authorizationServiceFixture, LoggingServiceFixture loggingServiceFixture)
        {
            _authorizationServiceFixture = authorizationServiceFixture;
            _loggingServiceFixture = loggingServiceFixture;
            StartUp();
        }
        #region Fields
        private AuthorizationServiceFixture _authorizationServiceFixture;
        private LoggingServiceFixture _loggingServiceFixture;
        private LogModelMockData _logModelMockData;

        private const string clientID = ClientIDs.loggingserver_testclient;
        private const string secret = ClientSecrets.loggingserver_testclient;
        private const string scope = LoggingServerScopes.full_access;
        private string accessToken;

        #endregion

        #region Overrides
        public override void StartUp()
        {
            base.StartUp();
            CleanUpLogs();
        }
        public override void InitVariables()
        {
            accessToken = _authorizationServiceFixture.AuthorizationService.CreateAccessTokenAsync(clientID, secret, scope).Result;
        }
        public override void InitMockData()
        {
            _logModelMockData = new LogModelMockData();
        }
        public override void CleanUpLogs()
        {
            var datas = (List<ErrorLog>)_loggingServiceFixture.LoggingServices.GetAllErrorLogAsync(accessToken).Result;
            if (datas.Count > 0)
            {
                _loggingServiceFixture.LoggingServices.DeleteAllErrorLogAsync(accessToken).Wait();
            }
        }
        public override void CleanUpMockData()
        {
            _logModelMockData = null;
        }
        #endregion

        [Fact]
        public void Post_WhenPassRightModel_LogToMongoDB()
        {
            // Pre Test
            var datas = _loggingServiceFixture.LoggingServices.GetAllErrorLogByDateAsync(DateTime.Today, accessToken).Result;
            Assert.Empty(datas);

            // Action
            _loggingServiceFixture.LoggingServices.LogAsync(_logModelMockData.GetErrorLogMockData(), IsDevMode: false).Wait();

            // Assert
            datas = _loggingServiceFixture.LoggingServices.GetAllErrorLogByDateAsync(DateTime.Today, accessToken).Result;
            Assert.NotEmpty(datas);
        }
        [Fact]
        public void Get_WhenIDPass_ReturnSingleLog()
        {
            // Pre Test
            List<ErrorLog> datas = (List<ErrorLog>)_loggingServiceFixture.LoggingServices.GetAllErrorLogByDateAsync(DateTime.Today, accessToken).Result;
            Assert.Empty(datas);

            // Action
            _loggingServiceFixture.LoggingServices.LogAsync(_logModelMockData.GetErrorLogMockData(), IsDevMode: false).Wait();

            // Assert Item Exists
            datas = (List<ErrorLog>)_loggingServiceFixture.LoggingServices.GetAllErrorLogByDateAsync(DateTime.Today, accessToken).Result;
            Assert.NotEmpty(datas);

            // Assert
            var response = _loggingServiceFixture.LoggingServices.GetErrorLogByIDAsync(datas[0].LogID.ToString(), accessToken).Result;
            Assert.True(LogComparer<ErrorLog>.Compare(datas[0], response));
        }
        [Fact]
        public void Get_WhenDatePass_ReturnEnumerablesOfLogs()
        {
            // Pre Test
            List<ErrorLog> datas = (List<ErrorLog>)_loggingServiceFixture.LoggingServices.GetAllErrorLogByDateAsync(DateTime.Today, accessToken).Result;
            Assert.Empty(datas);

            // Action
            _loggingServiceFixture.LoggingServices.LogAsync(_logModelMockData.GetErrorLogMockData(), IsDevMode: false).Wait();
            _loggingServiceFixture.LoggingServices.LogAsync(_logModelMockData.GetErrorLogMockData(), IsDevMode: false).Wait();
            _loggingServiceFixture.LoggingServices.LogAsync(_logModelMockData.GetErrorLogMockData(), IsDevMode: false).Wait();

            // Assert Item Exists
            datas = (List<ErrorLog>)_loggingServiceFixture.LoggingServices.GetAllErrorLogByDateAsync(DateTime.Today, accessToken).Result;
            Assert.NotEmpty(datas);

            // Assert Data Count
            Assert.Equal(3, datas.Count);
        }
        [Fact]
        public void Get_WhenNoParamaterPass_ReturnAllOfLogs()
        {
            // Pre Test
            List<ErrorLog> datas = (List<ErrorLog>)_loggingServiceFixture.LoggingServices.GetAllErrorLogByDateAsync(DateTime.Today, accessToken).Result;
            Assert.Empty(datas);

            // Action
            _loggingServiceFixture.LoggingServices.LogAsync(_logModelMockData.GetErrorLogMockData(), IsDevMode: false).Wait();
            _loggingServiceFixture.LoggingServices.LogAsync(_logModelMockData.GetErrorLogMockData(), IsDevMode: false).Wait();
            _loggingServiceFixture.LoggingServices.LogAsync(_logModelMockData.GetErrorLogMockData(), IsDevMode: false).Wait();

            // Assert Item Exists
            datas = (List<ErrorLog>)_loggingServiceFixture.LoggingServices.GetAllErrorLogByDateAsync(DateTime.Today, accessToken).Result;
            Assert.NotEmpty(datas);

            // Assert Data Count
            Assert.Equal(3, datas.Count);
        }
        [Fact]
        public void Delete_WhenIDParamaterPass_DeleteSingleLogs()
        {
            // Pre Test Init Data
            _loggingServiceFixture.LoggingServices.LogAsync(_logModelMockData.GetErrorLogMockData(), IsDevMode: false).Wait();
            _loggingServiceFixture.LoggingServices.LogAsync(_logModelMockData.GetErrorLogMockData(), IsDevMode: false).Wait();
            _loggingServiceFixture.LoggingServices.LogAsync(_logModelMockData.GetErrorLogMockData(), IsDevMode: false).Wait();

            List<ErrorLog> datas = (List<ErrorLog>)_loggingServiceFixture.LoggingServices.GetAllErrorLogAsync(accessToken).Result;
            Assert.NotEmpty(datas);

            // Assert Data Count
            Assert.Equal(3, datas.Count);

            var searchedID = datas[0].LogID;

            // Action
            _loggingServiceFixture.LoggingServices.DeleteErrorLogByIDAsync(searchedID, accessToken).Wait();

            // Assert Item Exists
            datas = (List<ErrorLog>)_loggingServiceFixture.LoggingServices.GetAllErrorLogAsync(accessToken).Result;
            Assert.NotEmpty(datas);

            // Assert Data Count
            Assert.Equal(2, datas.Count);

            // Assert Data Deleted
            var data = _loggingServiceFixture.LoggingServices.GetErrorLogByIDAsync(searchedID, accessToken).Result;
            Assert.Null(data);
        }
        [Fact]
        public void Delete_WhenIDParamaterPassNotExists_DeleteSingleLogs()
        {
            // Pre Test
            // Verify Data Not Exists
            var datas = _loggingServiceFixture.LoggingServices.GetAllErrorLogAsync(accessToken).Result;
            Assert.Empty(datas);

            var searchedID = Guid.NewGuid().ToString();

            // Action

            string expectedMessage = JsonConvert.SerializeObject(new { error = Constants.NetStandard.ExceptionMessage.ITEM_NOT_FOUND });

            var response = _loggingServiceFixture.LoggingServices.DeleteErrorLogByIDAsync(searchedID, accessToken).Result;

            Assert.Equal(expectedMessage, response.Content.ReadAsStringAsync().Result);

        }
        [Fact]
        public void Delete_WhenNoParamaterPass_DeleteAllOfLogs()
        {
            // Pre Test Init Data
            _loggingServiceFixture.LoggingServices.LogAsync(_logModelMockData.GetErrorLogMockData(), IsDevMode: false).Wait();
            _loggingServiceFixture.LoggingServices.LogAsync(_logModelMockData.GetErrorLogMockData(), IsDevMode: false).Wait();
            _loggingServiceFixture.LoggingServices.LogAsync(_logModelMockData.GetErrorLogMockData(), IsDevMode: false).Wait();

            List<ErrorLog> datas = (List<ErrorLog>)_loggingServiceFixture.LoggingServices.GetAllErrorLogAsync(accessToken).Result;
            Assert.NotEmpty(datas);

            // Assert Data Count
            Assert.Equal(3, datas.Count);

            // Action
            _loggingServiceFixture.LoggingServices.DeleteAllErrorLogAsync(accessToken).Wait();

            // Assert Item Exists
            datas = (List<ErrorLog>)_loggingServiceFixture.LoggingServices.GetAllErrorLogAsync(accessToken).Result;
            Assert.Empty(datas);
        }
        [Fact]
        public void Delete_WhenItemNotExists_ThrowItemNotFoundException()
        {
            // Pre Test
            // Verify Data Not Exists
            var datas = _loggingServiceFixture.LoggingServices.GetAllErrorLogAsync(accessToken).Result;
            Assert.Empty(datas);

            // Assert Action Throw Exception
            // Action
            string expectedMessage = JsonConvert.SerializeObject(new { error = Constants.NetStandard.ExceptionMessage.ITEM_NOT_FOUND });

            var response = _loggingServiceFixture.LoggingServices.DeleteAllErrorLogAsync(accessToken).Result;

            Assert.Equal(expectedMessage, response.Content.ReadAsStringAsync().Result);

        }
    }
}
