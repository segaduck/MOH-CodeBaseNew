using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;

namespace ES.Commons
{
  /// <summary>
  /// Converts a <see cref="DateTime"/> to and from the AD date string format (e.g. 2008-04-12 12:53:00).
  /// </summary>
  public class MyJsonDateTimeConverter : DateTimeConverterBase
  {
    private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";

    private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
    private string _dateTimeFormat;
    private CultureInfo _culture;

    /// <summary>
    /// 採用預設格式(yyyy-MM-dd HH:mm:ss)的建構子
    /// </summary>
    public MyJsonDateTimeConverter()
    {

    }
    
    /// <summary>
    /// 可指定 DateTime 格式字串的建構子
    /// </summary>
    /// <param name="dateTimeFormat"></param>
    public MyJsonDateTimeConverter(string dateTimeFormat)
    {
        if (!string.IsNullOrEmpty(dateTimeFormat))
        {
            this._dateTimeFormat = dateTimeFormat;
        }
    }

    /// <summary>
    /// Gets or sets the date time styles used when converting a date to and from JSON.
    /// </summary>
    /// <value>The date time styles used when converting a date to and from JSON.</value>
    public DateTimeStyles DateTimeStyles
    {
      get { return _dateTimeStyles; }
      set { _dateTimeStyles = value; }
    }

    /// <summary>
    /// Gets or sets the date time format used when converting a date to and from JSON.
    /// </summary>
    /// <value>The date time format used when converting a date to and from JSON.</value>
    public string DateTimeFormat
    {
      get { return _dateTimeFormat ?? string.Empty; }
        set { _dateTimeFormat = (value != null ? value : DefaultDateTimeFormat); }
    }

    /// <summary>
    /// Gets or sets the culture used when converting a date to and from JSON.
    /// </summary>
    /// <value>The culture used when converting a date to and from JSON.</value>
    public CultureInfo Culture
    {
      get { return _culture ?? CultureInfo.CurrentCulture; }
      set { _culture = value; }
    }

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      string text;

      if (value is DateTime)
      {
        DateTime dateTime = (DateTime)value;

        text = dateTime.ToString(_dateTimeFormat ?? DefaultDateTimeFormat);
      }
#if !PocketPC && !NET20
      else if (value is DateTimeOffset)
      {
        DateTimeOffset dateTimeOffset = (DateTimeOffset)value;

        text = dateTimeOffset.ToString(_dateTimeFormat ?? DefaultDateTimeFormat);
      }
#endif
      else
      {
        throw new Exception("Unexpected value when converting date. Expected DateTime or DateTimeOffset, got {0}." );
      }

      writer.WriteValue(text);
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      if (reader.TokenType == JsonToken.Null)
      {
         throw new Exception("Cannot convert null value to {0}." );
      }

      if (reader.TokenType != JsonToken.String)
        throw new Exception("Unexpected token parsing date. Expected String, got {0}.");

      string dateText = reader.Value.ToString();


#if !PocketPC && !NET20
      if (objectType == typeof(DateTimeOffset))
      {
        if (!string.IsNullOrEmpty(_dateTimeFormat))
          return DateTimeOffset.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
        else
          return DateTimeOffset.Parse(dateText, Culture, _dateTimeStyles);
      }
#endif

      if (!string.IsNullOrEmpty(_dateTimeFormat))
        return DateTime.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
      else
        return DateTime.Parse(dateText, Culture, _dateTimeStyles);
    }
  }
}