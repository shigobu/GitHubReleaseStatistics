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
Task githubTask = StartStatistics(token);

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

//end of main method

/// <summary>
/// 統計取得を開始します。
/// </summary>
async Task StartStatistics(CancellationToken token)
{
    using(HttpClient httpClient = new HttpClient())
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(1000);
            //throw new Exception("えくせぷしょん");
            //break;
        }
    }
}

