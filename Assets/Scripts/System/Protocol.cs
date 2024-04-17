using System.Collections.Generic;
using BackEnd.Tcp;
using UnityEngine;

namespace Protocol
{
    // �̺�Ʈ Ÿ��
    public enum Type : sbyte
    {
        Key = 0,        // Ű(���� ���̽�ƽ) �Է�
        PlayerMove,     // �÷��̾� �̵�
        PlayerRotate,   // �÷��̾� ȸ��
        PlayerAttack,   // �÷��̾� ����
        PlayerDamaged,  // �÷��̾� ������ ����
        PlayerNoMove,   // �÷��̾� �̵� ����
        PlayerNoRotate, // �÷��̾� ȸ�� ����
        bulletInfo,

        AIPlayerInfo,   // AI�� �����ϴ� ��� AI ����
        LoadRoomScene,      // �� ������ ��ȯ
        LoadGameScene,      // �ΰ��� ������ ��ȯ
        StartCount,     // ���� ī��Ʈ
        GameStart,      // ���� ����
        GameEnd,        // ���� ����
        GameSync,       // �÷��̾� ������ �� ���� ���� ��Ȳ ��ũ
        Max
    }
    // ���̽�ƽ Ű �̺�Ʈ �ڵ�
    public static class KeyEventCode
    {
        public const int NONE = 0;
        public const int MOVE = 1;      // �̵� �޽���
        //public const int ATTACK = 2;    // ���� �޽���
        public const int NO_MOVE = 4;   // �̵� ���� �޽���
    }
    public class Message
    {
        public Type type;

        public Message(Type type)
        {
            this.type = type;
        }
    }
    public class KeyMessage : Message
    {
        public int keyData;
        public float x;
        public float y;
        public float z;

        public KeyMessage(int data, Vector3 pos) : base(Type.Key)
        {
            this.keyData = data;
            this.x = pos.x;
            this.y = pos.y;
            this.z = pos.z;
        }
    }
    public class PlayerMoveMessage : Message
    {
        public SessionId playerSession;
        public float xPos;
        public float yPos;
        public float zPos;
        public float xDir;
        public float yDir;
        public float zDir;
        public PlayerMoveMessage(SessionId session, Vector3 pos, Vector3 dir) : base(Type.PlayerMove)
        {
            this.playerSession = session;
            this.xPos = pos.x;
            this.yPos = pos.y;
            this.zPos = pos.z;
            this.xDir = dir.x;
            this.yDir = dir.y;
            this.zDir = dir.z;
        }
    }

    public class LoadGameSceneMessage : Message
    {
        public LoadGameSceneMessage() : base(Type.LoadGameScene)
        {

        }
    }
    public class GameStartMessage : Message
    {
        public GameStartMessage() : base(Type.GameStart) { }
    }

    public class GameSyncMessage : Message
    {
        public SessionId host;
        public int count = 0;
        public float[] xPos = null;
        public float[] zPos = null;
        public bool[] onlineInfo = null;

        public GameSyncMessage(SessionId host, int count, float[] x, float[] z, bool[] online) : base(Type.GameSync)
        {
            this.host = host;
            this.count = count;
            this.xPos = new float[count];
            this.zPos = new float[count];
            this.onlineInfo = new bool[count];

            for (int i = 0; i < count; ++i)
            {
                xPos[i] = x[i];
                zPos[i] = z[i];
                onlineInfo[i] = online[i];
            }
        }
    }
}