using System.Net.Http;

var httpClient = new HttpClient();
var url = "ССЫЛКА_НА_РАСПИСАНИЕ";

var html = await httpClient.GetStringAsync(url);