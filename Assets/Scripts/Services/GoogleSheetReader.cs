using System;
using System.Collections.Generic;

using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

using UnityEngine;
using System.IO;

public class GoogleSheetReader
{
    private static readonly string spreadsheetId = "12acMQ8UTlDRHP0NvzSGVLYKb9QMhw2AjD9EKXTQug3U";
    private static readonly string jsonPath = "/StreamingAssets/Credentials/xplora-368713-9739f2b0a7e8.json";
    
    private static readonly SheetsService service;

    static GoogleSheetReader()
    {
        string fullJsonPath = Application.dataPath + jsonPath;
        Stream jsonCreds = (Stream)File.Open(fullJsonPath, FileMode.Open);
        ServiceAccountCredential credential = ServiceAccountCredential.FromServiceAccountData(jsonCreds);

        service = new SheetsService(
            new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
            }
        );
    }

    public IList<IList<object>> GetSheetRange(string sheetNameAndRange)
    {
        SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheetId, sheetNameAndRange);

        ValueRange response = request.Execute();
        IList<IList<object>> values = response.Values;
        if (values != null && values.Count > 0)
        {
            return values;
        }
        else
        {
            Debug.Log("No data found.");
            return null;
        }
    }
}