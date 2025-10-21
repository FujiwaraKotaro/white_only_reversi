using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // TextMeshPro�̖��O��Ԃ��C���|�[�g

public class Text : MonoBehaviour
{
    public TMP_Text winner;  // ���҂��������߂̕ϐ�
    public TMP_Text resultScore;  // �X�R�A���������߂̕ϐ�
    public TMP_Text space;  // �X�y�[�X�������ē����������߂̕ϐ�
    public TMP_Text timer;  // �^�C�}�[��\�����邽�߂̕ϐ�
    private MainManeger mainmManager;

    void Start()
    {
        mainmManager = FindObjectOfType<MainManeger>();
        // �ϐ��̒l��TMP�̃e�L�X�g�ɕ\��
        resultScore.text = "";
        space.text = "";
        winner.text = "";
    }

    private void Update()
    {
        mainmManager = FindObjectOfType<MainManeger>();


        timer.text = "�c��" + (5 - Mathf.Round(mainmManager.playerTimer * 1f) / 1f).ToString() + "�b";


        resultScore.text = "���Ȃ� " + mainmManager.whiteCountResult.ToString() + "\nCPU  " + mainmManager.blackCountResult.ToString();

        if (mainmManager.gameoverFlag == true)
        {
         // 1�����̏����A2�����̏����A3�����������Ȃǂ̔�����Ӗ�����

            switch (mainmManager.winner)
            {
                case 1:
                    // �������������ꍇ�̏���
                    winner.text = "���Ȃ��̏���";
                    break;

                case 2:
                    // �������������ꍇ�̏���
                    winner.text = "CPU�̏���";
                    break;

                case 3:
                    // ���������̏ꍇ�̏���
                    winner.text = "��������";
                    break;

                default:
                    // ���̑��iwinner��1, 2, 3�ȊO�̒l�̏ꍇ�j
                   �@winner.text = "�G���[";
                    break;
            }

            resultScore.text = "���Ȃ� " + mainmManager.whiteCountResult.ToString() + "\nCPU  " + mainmManager.blackCountResult.ToString();
            space.text = "�X�y�[�X��������\n�^�C�g���ɖ߂�";
            timer.text = "";
            Destroy(GameObject.Find("line"));
            Destroy(GameObject.Find("line(1)"));
        }
    }
}
