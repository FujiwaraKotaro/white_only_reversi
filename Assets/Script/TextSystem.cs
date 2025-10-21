using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // TextMeshProの名前空間をインポート

public class Text : MonoBehaviour
{
    public TMP_Text winner;  // 勝者を書くための変数
    public TMP_Text resultScore;  // スコアを書くための変数
    public TMP_Text space;  // スペースを押す案内を書くための変数
    public TMP_Text timer;  // タイマーを表示するための変数
    private MainManeger mainmManager;

    void Start()
    {
        mainmManager = FindObjectOfType<MainManeger>();
        // 変数の値をTMPのテキストに表示
        resultScore.text = "";
        space.text = "";
        winner.text = "";
    }

    private void Update()
    {
        mainmManager = FindObjectOfType<MainManeger>();


        timer.text = "残り" + (5 - Mathf.Round(mainmManager.playerTimer * 1f) / 1f).ToString() + "秒";


        resultScore.text = "あなた " + mainmManager.whiteCountResult.ToString() + "\nCPU  " + mainmManager.blackCountResult.ToString();

        if (mainmManager.gameoverFlag == true)
        {
         // 1が白の勝利、2が黒の勝利、3が引き分けなどの判定を意味する

            switch (mainmManager.winner)
            {
                case 1:
                    // 白が勝利した場合の処理
                    winner.text = "あなたの勝ち";
                    break;

                case 2:
                    // 黒が勝利した場合の処理
                    winner.text = "CPUの勝ち";
                    break;

                case 3:
                    // 引き分けの場合の処理
                    winner.text = "引き分け";
                    break;

                default:
                    // その他（winnerが1, 2, 3以外の値の場合）
                   　winner.text = "エラー";
                    break;
            }

            resultScore.text = "あなた " + mainmManager.whiteCountResult.ToString() + "\nCPU  " + mainmManager.blackCountResult.ToString();
            space.text = "スペースを押して\nタイトルに戻る";
            timer.text = "";
            Destroy(GameObject.Find("line"));
            Destroy(GameObject.Find("line(1)"));
        }
    }
}
