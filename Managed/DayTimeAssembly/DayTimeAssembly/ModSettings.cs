extern alias Newton;
using Newton::Newtonsoft.Json;
using Newton::Newtonsoft.Json.Converters;
using System;
using System.IO;
using UnityEngine;

[JsonConverter(typeof(StringEnumConverter))]
public class ModConfig<T>
{
    public ModConfig(string name)
    {
        _filename = name;
    }

    string _filename = null;

    string SettingsPath
    {
        get
        {
            return Path.Combine(Path.Combine(Application.persistentDataPath, "Modsettings"), _filename + ".json");
        }
    }

    public T Settings
    {
        get
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(Activator.CreateInstance<T>(), Formatting.Indented, new StringEnumConverter()));
                }

                return JsonConvert.DeserializeObject<T>(File.ReadAllText(SettingsPath));
            }
            catch (Exception ex)
            {
                if (ex is JsonSerializationException)
                {
                    Debug.LogFormat("[DayTime] An error was detected within the settings file, resetting...");
                    File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(Activator.CreateInstance<T>(), Formatting.Indented, new StringEnumConverter()));
                    return JsonConvert.DeserializeObject<T>(File.ReadAllText(SettingsPath));
                }
                return Activator.CreateInstance<T>();
            }
        }

        set
        {
            if (value.GetType() == typeof(T))
            {
                File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(value, Formatting.Indented, new StringEnumConverter()));
            }
        }
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(Settings, Formatting.Indented);
    }
}