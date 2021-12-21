// See https://aka.ms/new-console-template for more information
const string inputWateString = "> ";

Console.WriteLine("ようこそ、GitHub Release統計アプリへ。");
Console.WriteLine("毎時0分にReleaseのダウンロード数を取得し、直前の一時間にダウンロードされた回数を記録します。");
Console.WriteLine();

string githubUserName;
while(true)
{
    Console.WriteLine("ユーザー名の入力");
    Console.Write(inputWateString);
    githubUserName = Console.ReadLine() ?? "";
    if (string.IsNullOrWhiteSpace(githubUserName))
    {
        Console.CursorLeft = 0;
        Console.CursorTop -= 2;
        continue;
    }
    else
    {
        break;
    }
}

string githubRepositoryName;
while(true)
{
    Console.WriteLine("リポジトリ名の入力");
    Console.Write(inputWateString);
    githubRepositoryName = Console.ReadLine() ?? "";
    if (string.IsNullOrWhiteSpace(githubRepositoryName))
    {
        Console.CursorLeft = 0;
        Console.CursorTop -= 2;
        continue;
    }
    else
    {
        break;
    }
}

Console.WriteLine($"ユーザー名:{githubUserName} リポジトリ名:{githubRepositoryName} で接続します。");

var tokenSource = new CancellationTokenSource();
CancellationToken token = tokenSource.Token;
Task githubTask = Task.Run(() => StartStatistics(token));

Task readLineTask = Task.Run(() =>
{
    Console.WriteLine("終了するには、\"exit\"と入力してください。");
    while (true)
    {
        Console.Write(inputWateString);
        string readString = Console.ReadLine() ?? "";
        if (readString == "exit") 
        { 
            tokenSource.Cancel();
            break;
        }
    }
});

try
{
    await githubTask;
    await readLineTask;
    Console.WriteLine("ユーザー操作により終了");
}
catch (Exception ex)
{
    ConsoleColor defaultColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine();
    Console.WriteLine(ex.ToString());
    Console.WriteLine("エラー発生により終了");
    Console.ForegroundColor = defaultColor;
}
finally
{
    tokenSource.Dispose();
}

return;
//end of main method

/// <summary>
/// 統計取得を開始します。
/// </summary>
async Task StartStatistics(CancellationToken token)
{
    using(HttpClient httpClient = new HttpClient())
    {
        bool needsExecution = true;

        while (!token.IsCancellationRequested)
        {
            //毎時0分に実行
            if (DateTime.Now.Second != 0)
            {
                needsExecution = true;
                continue;
            }

            if (!needsExecution)
            {
                continue;
            }

            Console.WriteLine(DateTime.Now);

            if (!File.Exists(GetPrevReleasesInformationJson()))
            {
                string releaseInfoJson = await GetReleaseInfoJson(httpClient, githubUserName, githubRepositoryName);
                using (StreamWriter writer = new StreamWriter(GetPrevReleasesInformationJson()))
                {
                    writer.Write(releaseInfoJson);
                }
            }

            needsExecution = false;
        }
    }
}


async Task<string> GetReleaseInfoJson(HttpClient httpClient, string userName, string repositoryName)
{
    var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{userName}/{repositoryName}/releases");
    request.Headers.Add("Accept", "application/vnd.github.v3+json");
    request.Headers.Add("User-Agent", "shigobu");
    using (var result = await httpClient.SendAsync(request))
    {
        return await result.Content.ReadAsStringAsync();
    }
}

/// <summary>
/// このアプリが置いてあるDirectoryを取得します。
/// </summary>
/// <returns>このアプリが置いてあるDirectory</returns>
string GetThisAppDirectory()
{
    string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
    return Path.GetDirectoryName(appPath);
}

/// <summary>
/// githubから取得できる前回のReleases情報のJsonファイル名
/// </summary>
/// <returns></returns>
string GetPrevReleasesInformationJson()
{
    return Path.Combine(GetThisAppDirectory(), "PrevReleasesInformation.json");
}
