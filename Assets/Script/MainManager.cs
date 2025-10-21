using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityroom.Api;


public class MainManeger : MonoBehaviour
{
    private SceneChange sceneChange;

    // ���Ɣ��̃f�B�X�N�A�|�C���^�[�̃v���n�u��ݒ�
    public GameObject blackDisk;
    public GameObject whiteDisk;
    public GameObject pointer;

    //�X�R�A�\���̂��߂̕ϐ�
    public int whiteCountResult = 0;
    public int blackCountResult = 0;

    // ���݂̃v���C���[��\���ϐ�
    public int currentPlayer = WHITE;

    //gameover�̂��߂̃t���O
    public bool gameoverFlag = false;

    //���s����̂��߂̕ϐ�
    public int winner;

    // 8x8�̃{�[�h�z��B�����l�͂��ׂ�0�i��j
    private int[,] board = new int[8, 8];

    // �{�[�h�̏�Ԃ�\���萔
    private const int EMPTY = 0;
    private const int WHITE = 1;
    private const int BLACK = 2;

    // �΂𗠕Ԃ����߂�8�̕����i�㉺���E����ю΂߁j
    private readonly int[,] directions = new int[,] {
        { 0, 1 },   // ��
        { 1, 1 },   // �E��
        { 1, 0 },   // �E
        { 1, -1 },  // �E��
        { 0, -1 },  // ��
        { -1, -1 }, // ����
        { -1, 0 },  // ��
        { -1, 1 }   // ����
    };


    // �v���C���[�̃^�C�}�[
    public float playerTimer = 0f;
    private const float TURN_TIME_LIMIT = 5f;

    private bool isProcessing = false;


    // Start�̓Q�[���J�n���Ɉ�x�����Ă΂��
    void Start()
    {
        sceneChange = FindObjectOfType<SceneChange>();
        //�X�R�A�J�E���g��������
        whiteCountResult = 0;�@
        blackCountResult = 0;

    // �����z�u��ݒ�i�����ɔ���4�̃f�B�X�N��z�u�j
    board[3, 3] = WHITE;
        board[3, 4] = BLACK;
        board[4, 4] = WHITE;
        board[4, 3] = BLACK;

        // �Ֆʂ̏�ԂɊ�Â��ăf�B�X�N�𐶐�
        UpdateBoardVisuals();

        // �^�C�}�[������
        playerTimer = 0f;
    }



    // Update�̓t���[�����ƂɌĂ΂��i���t���[�����s�j
    void Update()
    {
        // �Q�[���I���`�F�b�N
        if (gameoverFlag)
        {

            // �{�[�hNo1�ɃX�R�A123.45f�𑗐M����B
            UnityroomApiClient.Instance.SendScore(1, (float)blackCountResult, ScoreboardWriteMode.HighScoreDesc);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("�X�y�[�X�L�[��������܂����B");
                sceneChange.ChangeTitle();
            }
            return;
        }


        // �v���C���[�̃^�[�����Ԃ��v��
        playerTimer += Time.deltaTime;

        // 5�b�𒴂����ꍇ�A�Q�[���I��
        if (playerTimer > TURN_TIME_LIMIT)
        {
            Debug.Log("5�b�ȓ��Ɏ肪�ł���Ȃ��������߁A�Q�[���I���B");
            gameoverFlag = true;
            ShowWinner();
            return;
        }

        // �p�X�`�F�b�N
        if (PassCheck() && PassCheck())
        {
            Debug.Log("�Q�[���I��");
            gameoverFlag = true;
            ShowWinner();
        }

        // ���̃^�[���ł���΃����_���ɔz�u
        if (currentPlayer == BLACK && !isProcessing)
        {
            isProcessing = true;
            StartCoroutine(PlaceRandomDisk());
            // �^�C�}�[�����Z�b�g
        }


        // �}�E�X���N���b�N�������ꂽ���̏���
        if (Input.GetMouseButtonDown(0))
        {
            // �}�E�X�̃��[���h���W���擾���A�{�[�h��̈ʒu�ɕϊ�
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = Mathf.RoundToInt(mousePos.x);
            int y = Mathf.RoundToInt(mousePos.y);

            // �N���b�N�����ʒu���{�[�h�͈͓̔��ł��邩�m�F
            if (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                // �N���b�N�����ʒu����ł���A�L���Ȏ�ł��邩�m�F
                if (board[x, y] == EMPTY && IsValidMove(x, y))
                {
                    // �f�B�X�N��z�u���A�΂𗠕Ԃ�
                    PlaceDisk(x, y);
                    FlipDisks(x, y);

                    // �v���C���[��؂�ւ���
                    SwitchPlayer();

                    // �Ֆʂ̎��o���X�V
                    UpdateBoardVisuals();


                    // Disk�̖������J�E���g����
                    DiskCount();

                    // �^�C�}�[�����Z�b�g
                    playerTimer = 0f;
                }
            }
        }

        

    }




    IEnumerator PlaceRandomDisk()
    {
        yield return new WaitForSeconds(0.5f); // 0.5�b�̒x����ǉ�

        List<Vector2Int> validMoves = new List<Vector2Int>();

        // �L���Ȏ�����X�g�A�b�v
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

        // �����_���ȗL���Ȏ��I��
        if (validMoves.Count > 0)
        {
            Vector2Int move = validMoves[Random.Range(0, validMoves.Count)];
            PlaceDisk(move.x, move.y);
            FlipDisks(move.x, move.y);
            SwitchPlayer();
            UpdateBoardVisuals();
            DiskCount();
        }

        // �^�C�}�[�����Z�b�g
        playerTimer = 0f;
        isProcessing = false;
    }


    /// <summary>
    /// �L���Ȏ肩�ǂ������`�F�b�N����֐�
    /// </summary>
    /// <param name="x">�{�[�h��x���W</param>
    /// <param name="y">�{�[�h��y���W</param>
    /// <returns>�L���Ȏ�ł����true�A�����łȂ����false</returns>
    bool IsValidMove(int x, int y)
    {
        // �u�����Ƃ��Ă���}�X����łȂ��ꍇ�͖���
        if (board[x, y] != EMPTY)
            return false;

        // �e�����ɑ΂��ă`�F�b�N
        for (int d = 0; d < directions.GetLength(0); d++)
        {
            int dx = directions[d, 0];
            int dy = directions[d, 1];
            int nx = x + dx;
            int ny = y + dy;//�N���b�N�������Wx,y�ɂW�������̍��W�𑫂��Ă��ꂼ�꒲�ׂ�

            bool hasOpponentDiskBetween = false;

            // ����̐΂���������i��
            while (nx >= 0 && nx < 8 && ny >= 0 && ny < 8 && board[nx, ny] == GetOpponent())
            {
                nx += dx;
                ny += dy;
                hasOpponentDiskBetween = true;
            }

            // �Ō�Ɏ����̐΂�����A���r���ɑ���̐΂��������ꍇ
            if (hasOpponentDiskBetween && nx >= 0 && nx < 8 && ny >= 0 && ny < 8 && board[nx, ny] == currentPlayer)
            {
                return true;
            }
        }

        // �ǂ̕����ł������𖞂����Ȃ��ꍇ�͖���
        return false;
    }



    /// <summary>
    /// �f�B�X�N��z�u����֐�
    /// </summary>
    /// <param name="x">�{�[�h��x���W</param>
    /// <param name="y">�{�[�h��y���W</param>
    void PlaceDisk(int x, int y)
    {
        // ���݂̃v���C���[�ɉ����ăf�B�X�N��z�u
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
    /// �΂𗠕Ԃ��֐�
    /// </summary>
    /// <param name="x">�{�[�h��x���W</param>
    /// <param name="y">�{�[�h��y���W</param>
    void FlipDisks(int x, int y)
    {
        // �e�����ɑ΂��ă`�F�b�N
        for (int d = 0; d < directions.GetLength(0); d++)
        {
            int dx = directions[d, 0];
            int dy = directions[d, 1];
            int nx = x + dx;
            int ny = y + dy;

            List<Vector2Int> disksToFlip = new List<Vector2Int>();//�񎟌����W�����郊�X�g

            // ����̐΂���������i�݁A���Ԃ��ׂ��΂����X�g�ɒǉ�
            while (nx >= 0 && nx < 8 && ny >= 0 && ny < 8 && board[nx, ny] == GetOpponent())
            {
                disksToFlip.Add(new Vector2Int(nx, ny));
                nx += dx;
                ny += dy;
            }

            // �Ō�Ɏ����̐΂�����A���r���ɑ���̐΂��������ꍇ
            if (nx >= 0 && nx < 8 && ny >= 0 && ny < 8 && board[nx, ny] == currentPlayer)
            {
                // ���X�g�ɒǉ����ꂽ�΂𗠕Ԃ�
                foreach (Vector2Int pos in disksToFlip)
                {
                    board[pos.x, pos.y] = currentPlayer;
                }
            }
        }
    }

    /// <summary>
    /// �v���C���[��؂�ւ���֐�
    /// </summary>
    void SwitchPlayer()
    {
        currentPlayer = (currentPlayer == WHITE) ? BLACK : WHITE;
    }



    /// <summary>
    /// ����̃v���C���[���擾����֐�
    /// </summary>
    /// <returns>����̃v���C���[�iWHITE�܂���BLACK�j</returns>
    int GetOpponent()
    {
        return (currentPlayer == WHITE) ? BLACK : WHITE;
    }



    /// <summary>
    /// �Ֆʂ̎��o���X�V����֐�
    /// �����̃f�B�X�N���폜���Aboard�z��Ɋ�Â��čĔz�u����
    /// </summary>
    void UpdateBoardVisuals()
    {
        // �����̃f�B�X�N�����ׂč폜
        GameObject[] existingDisks = GameObject.FindGameObjectsWithTag("Disk");
        foreach (GameObject disk in existingDisks)
        {
            Destroy(disk);
        }

        // �����̃|�C���^�[�����ׂč폜
        GameObject[] existingPointers = GameObject.FindGameObjectsWithTag("Pointer");
        foreach (GameObject pointerObj in existingPointers)
        {
            Destroy(pointerObj);
        }


        // board�z��Ɋ�Â��ăf�B�X�N���Ĕz�u
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (board[i, j] == WHITE)
                {
                    // ���̃f�B�X�N��z�u���A�^�O��ݒ�
                    GameObject disk = Instantiate(whiteDisk, new Vector3(i, j, 0), Quaternion.identity);
                    disk.tag = "Disk";
                }
                else if (board[i, j] == BLACK)
                {
                    // ���̃f�B�X�N��z�u���A�^�O��ݒ�
                    GameObject disk = Instantiate(blackDisk, new Vector3(i, j, 0), Quaternion.identity);
                    disk.tag = "Disk";
                }
            }
        }


        // �{�[�h��̂��ׂĂ̈ʒu���m�F
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // ���̏ꏊ���L���Ȏ�ł���΃|�C���^�[��z�u
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
        // �L���Ȏ肪���邩�ǂ����������t���O
        bool hasValidMove = false;

        // 8x8�̃{�[�h�S�̂𑖍����ėL���Ȏ肪���邩�`�F�b�N
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // ���łɃf�B�X�N���u����Ă���ꏊ�͖���
                if (board[x, y] != EMPTY)
                {
                    continue;
                }

                // �L���Ȏ肪���������ꍇ
                if (IsValidMove(x, y))
                {
                    hasValidMove = true;
                    break; // �����̃��[�v���I��
                }
            }

            // �L���Ȏ肪����������O���̃��[�v���I��
            if (hasValidMove)
            {
                break;
            }
        }

        // �L���Ȏ肪�Ȃ��ꍇ�̓v���C���[��؂�ւ��ăp�X�Ƃ���
        if (!hasValidMove)
        {
            SwitchPlayer();
            Debug.Log("�p�X�ł�");

            // �|�C���^�[���ĕ\�����邽�߂�UpdateBoardVisuals���Ăяo���A�������Ȃ��ƃf�B�X�N��ς����Ƃ��Ƀ|�C���^�[���o�Ă��Ȃ�
            UpdateBoardVisuals();

            return true; // �p�X�����������ꍇ
        }

        return false; // �p�X���������Ȃ������ꍇ
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

    //���s�@�\
    void ShowWinner()
    {
        DiskCount();

        if (whiteCountResult > blackCountResult)
        {
            winner = 1;
            Debug.Log("���̏����I");
        }
        else if (blackCountResult > whiteCountResult)
        {
            winner = 2;
            Debug.Log("���̏����I");
        }
        else
        {
            winner = 3;
            Debug.Log("���������I");
        }
    }

       
}
