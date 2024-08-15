using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Tables;

public class LocalizationTest : MonoBehaviour
{
    AsyncOperationHandle m_InitializeOperation;
    private Locale _chineseLocale;
    private Locale _englishLocale;
    private Locale _japaneseLocale;

    void Start()
    {
        // SelectedLocaleAsync will ensure that the locales have been initialized and a locale has been selected.
        m_InitializeOperation = LocalizationSettings.SelectedLocaleAsync;
        if (m_InitializeOperation.IsDone)
        {
            InitializeCompleted(m_InitializeOperation);
        }
        else
        {
            m_InitializeOperation.Completed += InitializeCompleted;
        }
    }

    private void OnDestroy()
    {
        m_InitializeOperation.Completed -= InitializeCompleted;
    }

    void InitializeCompleted(AsyncOperationHandle obj)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        for (int i = 0; i < locales.Count; ++i)
        {
            var locale = locales[i];
            switch (locale.LocaleName)
            {
                case "Chinese (Simplified) (zh-Hans)":
                    _chineseLocale = locale;
                    break;
                case "English (en)":
                    _englishLocale = locale;
                    break;
                case "Japanese (ja)":
                    _japaneseLocale = locale;
                    break;
            }
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 50), "中文"))
        {
            LocalizationSettings.Instance.SetSelectedLocale(_chineseLocale);
        }

        if (GUI.Button(new Rect(0, 60, 100, 50), "英文"))
        {
            LocalizationSettings.Instance.SetSelectedLocale(_englishLocale);
        }
        if (GUI.Button(new Rect(0, 120, 100, 50), "日文"))
        {
            LocalizationSettings.Instance.SetSelectedLocale(_japaneseLocale);
        }

        if (GUI.Button(new Rect(0, 180, 100, 50), "hello"))
        {
            SetTextKey("id_hello");
        }

        if (GUI.Button(new Rect(0, 240, 100, 50), "bye"))
        {
            SetTextKey("id_bye");
        }

    }

    public TextMeshProUGUI text;
    private void SetTextKey(string _key)
    {
        key_Translate = _key;
        StartCoroutine(LoadStrings(_key));
    }
    IEnumerator LoadStrings(string _key)
    {
        // A string table may not be immediately available such as during initialization of the localization system or when a table has not been loaded yet.
        var loadingOperation = LocalizationSettings.StringDatabase.GetTableAsync("Lang_Base");
        yield return loadingOperation;

        if (loadingOperation.Status == AsyncOperationStatus.Succeeded)
        {
            var stringTable = loadingOperation.Result;
            text.text = stringTable.GetEntry(_key).GetLocalizedString();
        }
        else
        {
            Debug.LogError("Could not load String Table\n" + loadingOperation.OperationException.ToString());
        }
    }


    public LocalizedStringTable stringTable = new LocalizedStringTable { TableReference = "Lang_Base" };
    string m_TranslateStringHello;
    string key_Translate = "id_hello";
    private void OnEnable()
    {
        stringTable.TableChanged += LoadStrings;
    }

    private void OnDisable()
    {
        stringTable.TableChanged -= LoadStrings;
    }

    private void LoadStrings(StringTable _value)
    {
        m_TranslateStringHello = GetLocalizedString(_value, key_Translate);
        this.text.text = m_TranslateStringHello;
    }

    private string GetLocalizedString(StringTable _table, string _key)
    {
        var entry = _table.GetEntry(_key);
        return entry.GetLocalizedString();
    }


}
