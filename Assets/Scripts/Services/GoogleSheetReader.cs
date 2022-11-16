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
    private static readonly string jsonPath = "Credentials/key";
    private static readonly string serviceAccountId = "xplora-metadata-reader@xplora-368713.iam.gserviceaccount.com";


    private static readonly SheetsService service;

    static GoogleSheetReader()
    {
        string key = Resources.Load<TextAsset>(jsonPath).ToString();
        ServiceAccountCredential.Initializer initializer = new(
            serviceAccountId);
        ServiceAccountCredential credential = new(
            initializer.FromPrivateKey(key));

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