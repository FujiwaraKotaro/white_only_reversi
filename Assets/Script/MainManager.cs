using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityroom.Api;


public class MainManeger : MonoBehaviour
{
    private SceneChange sceneChange;

    // 黒と白のディスク、ポインターのプレハブを設定
    public GameObject blackDisk;
    public GameObject whiteDisk;
    public GameObject pointer;

    //スコア表示のための変数
    public int whiteCountResult = 0;
    public int blackCountResult = 0;

    // 現在のプレイヤーを表す変数
    public int currentPlayer = WHITE;

    //gameoverのためのフラグ
    public bool gameoverFlag = false;

    //勝敗決定のための変数
    public int winner;

    // 8x8のボード配列。初期値はすべて0（空）
    private int[,] board = new int[8, 8];

    // ボードの状態を表す定数
    private const int EMPTY = 0;
    private const int WHITE = 1;
    private const int BLACK = 2;

    // 石を裏返すための8つの方向（上下左右および斜め）
    private readonly int[,] directions = new int[,] {
        { 0, 1 },   // 上
        { 1, 1 },   // 右上
        { 1, 0 },   // 右
        { 1, -1 },  // 右下
        { 0, -1 },  // 下
        { -1, -1 }, // 左下
        { -1, 0 },  // 左
        { -1, 1 }   // 左上
    };


    // プレイヤーのタイマー
    public float playerTimer = 0f;
    private const float TURN_TIME_LIMIT = 5f;

    private bool isProcessing = false;


    // Startはゲーム開始時に一度だけ呼ばれる
    void Start()
    {
        sceneChange = FindObjectOfType<SceneChange>();
        //スコアカウントを初期化
        whiteCountResult = 0;　
        blackCountResult = 0;

    // 初期配置を設定（中央に白黒4つのディスクを配置）
    board[3, 3] = WHITE;
        board[3, 4] = BLACK;
        board[4, 4] = WHITE;
        board[4, 3] = BLACK;

        // 盤面の状態に基づいてディスクを生成
        UpdateBoardVisuals();

        // タイマー初期化
        playerTimer = 0f;
    }



    // Updateはフレームごとに呼ばれる（毎フレーム実行）
    void Update()
    {
        // ゲーム終了チェック
        if (gameoverFlag)
        {

            // ボードNo1にスコア123.45fを送信する。
            UnityroomApiClient.Instance.SendScore(1, (float)blackCountResult, ScoreboardWriteMode.HighScoreDesc);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("スペースキーが押されました。");
                sceneChange.ChangeTitle();
            }
            return;
        }


        // プレイヤーのターン時間を計測
        playerTimer += Time.deltaTime;

        // 5秒を超えた場合、ゲーム終了
        if (playerTimer > TURN_TIME_LIMIT)
        {
            Debug.Log("5秒以内に手が打たれなかったため、ゲーム終了。");
            gameoverFlag = true;
            ShowWinner();
            return;
        }

        // パスチェック
        if (PassCheck() && PassCheck())
        {
            Debug.Log("ゲーム終了");
            gameoverFlag = true;
            ShowWinner();
        }

        // 黒のターンであればランダムに配置
        if (currentPlayer == BLACK && !isProcessing)
        {
            isProcessing = true;
            StartCoroutine(PlaceRandomDisk());
            // タイマーをリセット
        }


        // マウス左クリックが押された時の処理
        if (Input.GetMouseButtonDown(0))
        {
            // マウスのワールド座標を取得し、ボード上の位置に変換
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = Mathf.RoundToInt(mousePos.x);
            int y = Mathf.RoundToInt(mousePos.y);

            // クリックした位置がボードの範囲内であるか確認
            if (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                // クリックした位置が空であり、有効な手であるか確認
                if (board[x, y] == EMPTY && IsValidMove(x, y))
                {
                    // ディスクを配置し、石を裏返す
                    PlaceDisk(x, y);
                    FlipDisks(x, y);

                    // プレイヤーを切り替える
                    SwitchPlayer();

                    // 盤面の視覚を更新
                    UpdateBoardVisuals();


                    // Diskの枚数をカウントする
                    DiskCount();

                    // タイマーをリセット
                    playerTimer = 0f;
                }
            }
        }

        

    }




    IEnumerator PlaceRandomDisk()
    {
        yield return new WaitForSeconds(0.5f); // 0.5秒の遅延を追加

        List<Vector2Int> validMoves = new List<Vector2Int>();

        // 有効な手をリストアップ
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] == EMPTY && IsValidMove(x, y))
                {
                    validMoves.Add(new Vector2Int(x, y));
                }
            }
        }

        // ランダムな有効な手を選択
        if (validMoves.Count > 0)
        {
            Vector2Int move = validMoves[Random.Range(0, validMoves.Count)];
            PlaceDisk(move.x, move.y);
            FlipDisks(move.x, move.y);
            SwitchPlayer();
            UpdateBoardVisuals();
            DiskCount();
        }

        // タイマーをリセット
        playerTimer = 0f;
        isProcessing = false;
    }


    /// <summary>
    /// 有効な手かどうかをチェックする関数
    /// </summary>
    /// <param name="x">ボードのx座標</param>
    /// <param name="y">ボードのy座標</param>
    /// <returns>有効な手であればtrue、そうでなければfalse</returns>
    bool IsValidMove(int x, int y)
    {
        // 置こうとしているマスが空でない場合は無効
        if (board[x, y] != EMPTY)
            return false;

        // 各方向に対してチェック
        for (int d = 0; d < directions.GetLength(0); d++)
        {
            int dx = directions[d, 0];
            int dy = directions[d, 1];
            int nx = x + dx;
            int ny = y + dy;//クリックした座標x,yに８方向分の座標を足してそれぞれ調べる

            bool hasOpponentDiskBetween = false;

            // 相手の石が続く限り進む
            while (nx >= 0 && nx < 8 && ny >= 0 && ny < 8 && board[nx, ny] == GetOpponent())
            {
                nx += dx;
                ny += dy;
                hasOpponentDiskBetween = true;
            }

            // 最後に自分の石があり、かつ途中に相手の石があった場合
            if (hasOpponentDiskBetween && nx >= 0 && nx < 8 && ny >= 0 && ny < 8 && board[nx, ny] == currentPlayer)
            {
                return true;
            }
        }

        // どの方向でも条件を満たさない場合は無効
        return false;
    }



    /// <summary>
    /// ディスクを配置する関数
    /// </summary>
    /// <param name="x">ボードのx座標</param>
    /// <param name="y">ボードのy座標</param>
    void PlaceDisk(int x, int y)
    {
        // 現在のプレイヤーに応じてディスクを配置
        if (currentPlayer == WHITE)
        {
            board[x, y] = WHITE;
        }
        else if (currentPlayer == BLACK)
        {
            board[x, y] = BLACK;
        }
    }



    /// <summary>
    /// 石を裏返す関数
    /// </summary>
    /// <param name="x">ボードのx座標</param>
    /// <param name="y">ボードのy座標</param>
    void FlipDisks(int x, int y)
    {
        // 各方向に対してチェック
        for (int d = 0; d < directions.GetLength(0); d++)
        {
            int dx = directions[d, 0];
            int dy = directions[d, 1];
            int nx = x + dx;
            int ny = y + dy;

            List<Vector2Int> disksToFlip = new List<Vector2Int>();//二次元座標を入れるリスト

            // 相手の石が続く限り進み、裏返すべき石をリストに追加
            while (nx >= 0 && nx < 8 && ny >= 0 && ny < 8 && board[nx, ny] == GetOpponent())
            {
                disksToFlip.Add(new Vector2Int(nx, ny));
                nx += dx;
                ny += dy;
            }

            // 最後に自分の石があり、かつ途中に相手の石があった場合
            if (nx >= 0 && nx < 8 && ny >= 0 && ny < 8 && board[nx, ny] == currentPlayer)
            {
                // リストに追加された石を裏返す
                foreach (Vector2Int pos in disksToFlip)
                {
                    board[pos.x, pos.y] = currentPlayer;
                }
            }
        }
    }

    /// <summary>
    /// プレイヤーを切り替える関数
    /// </summary>
    void SwitchPlayer()
    {
        currentPlayer = (currentPlayer == WHITE) ? BLACK : WHITE;
    }



    /// <summary>
    /// 相手のプレイヤーを取得する関数
    /// </summary>
    /// <returns>相手のプレイヤー（WHITEまたはBLACK）</returns>
    int GetOpponent()
    {
        return (currentPlayer == WHITE) ? BLACK : WHITE;
    }



    /// <summary>
    /// 盤面の視覚を更新する関数
    /// 既存のディスクを削除し、board配列に基づいて再配置する
    /// </summary>
    void UpdateBoardVisuals()
    {
        // 既存のディスクをすべて削除
        GameObject[] existingDisks = GameObject.FindGameObjectsWithTag("Disk");
        foreach (GameObject disk in existingDisks)
        {
            Destroy(disk);
        }

        // 既存のポインターをすべて削除
        GameObject[] existingPointers = GameObject.FindGameObjectsWithTag("Pointer");
        foreach (GameObject pointerObj in existingPointers)
        {
            Destroy(pointerObj);
        }


        // board配列に基づいてディスクを再配置
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (board[i, j] == WHITE)
                {
                    // 白のディスクを配置し、タグを設定
                    GameObject disk = Instantiate(whiteDisk, new Vector3(i, j, 0), Quaternion.identity);
                    disk.tag = "Disk";
                }
                else if (board[i, j] == BLACK)
                {
                    // 黒のディスクを配置し、タグを設定
                    GameObject disk = Instantiate(blackDisk, new Vector3(i, j, 0), Quaternion.identity);
                    disk.tag = "Disk";
                }
            }
        }


        // ボード上のすべての位置を確認
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // その場所が有効な手であればポインターを配置
                if (board[x, y] == EMPTY && IsValidMove(x, y))
                {
                    GameObject pointerObj = Instantiate(pointer, new Vector3(x, y, 0), Quaternion.identity);
                    pointerObj.tag = "Pointer";
                }
            }
        }
    }



    private bool PassCheck()
    {
        // 有効な手があるかどうかを示すフラグ
        bool hasValidMove = false;

        // 8x8のボード全体を走査して有効な手があるかチェック
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // すでにディスクが置かれている場所は無視
                if (board[x, y] != EMPTY)
                {
                    continue;
                }

                // 有効な手が見つかった場合
                if (IsValidMove(x, y))
                {
                    hasValidMove = true;
                    break; // 内側のループを終了
                }
            }

            // 有効な手が見つかったら外側のループも終了
            if (hasValidMove)
            {
                break;
            }
        }

        // 有効な手がない場合はプレイヤーを切り替えてパスとする
        if (!hasValidMove)
        {
            SwitchPlayer();
            Debug.Log("パスです");

            // ポインターを再表示するためにUpdateBoardVisualsを呼び出す、ここがないとディスクを変えたときにポインターが出てこない
            UpdateBoardVisuals();

            return true; // パスが発生した場合
        }

        return false; // パスが発生しなかった場合
    }

    void DiskCount()
    {
        int whiteCount = 0;
        int blackCount = 0;


        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] == WHITE) whiteCount++;
                if (board[x, y] == BLACK) blackCount++;
            }
        }

        whiteCountResult = whiteCount;
        blackCountResult = blackCount;
    }

    //勝敗機能
    void ShowWinner()
    {
        DiskCount();

        if (whiteCountResult > blackCountResult)
        {
            winner = 1;
            Debug.Log("白の勝利！");
        }
        else if (blackCountResult > whiteCountResult)
        {
            winner = 2;
            Debug.Log("黒の勝利！");
        }
        else
        {
            winner = 3;
            Debug.Log("引き分け！");
        }
    }

       
}
