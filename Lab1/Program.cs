using Practicum1;

void print_dict(Dictionary<string, float> dict, string user)
{
    Console.WriteLine($"__________ Withdrawal for {user} __________");
    Console.WriteLine();
    foreach (KeyValuePair<string, float> entry in dict)
        Console.WriteLine($"{entry.Key}: {entry.Value}");

    Console.WriteLine();
    Console.WriteLine();
}


async Task test_async_with_cancel()
{
    Emotion_detector t = new Emotion_detector();


    CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
    CancellationToken token = cancelTokenSource.Token;

    CancellationTokenSource cancelTokenSource1 = new CancellationTokenSource();
    CancellationToken token1 = cancelTokenSource1.Token;

    CancellationTokenSource cancelTokenSource2 = new CancellationTokenSource();
    CancellationToken token2 = cancelTokenSource2.Token;

    var user = t.Start("face1.jpg", token);
    var user1 = t.Start("face2.jpg", token1);
    var user2 = t.Start("face3.jpg", token2);

    cancelTokenSource.Cancel();
    cancelTokenSource1.Cancel();
    Thread.Sleep(2000);
    cancelTokenSource2.Cancel();


    await user;
    await user1;
    await user2;
    print_dict(user.Result, "user");
    print_dict(user1.Result, "user1");
    print_dict(user2.Result, "user2");
}

async Task test_async()
{
    Emotion_detector t = new Emotion_detector();

    CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
    CancellationToken token = cancelTokenSource.Token;

    CancellationTokenSource cancelTokenSource1 = new CancellationTokenSource();
    CancellationToken token1 = cancelTokenSource1.Token;

    CancellationTokenSource cancelTokenSource2 = new CancellationTokenSource();
    CancellationToken token2 = cancelTokenSource2.Token;

    var user = t.Start("face4.jpg", token);
    var user1 = t.Start("face5.jpg", token1);
    var user2 = t.Start("face6.jpg", token2);

    await user;
    await user1;
    await user2;
    print_dict(user.Result, "user");
    print_dict(user1.Result, "user1");
    print_dict(user2.Result, "user2");
}

Console.WriteLine("$Async Test$");
await test_async();

Console.WriteLine("$Async Test with CancellationTokens$");
await test_async_with_cancel();
