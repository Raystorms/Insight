﻿using Insight;
using UnityEngine;
using UnityEngine.UI;

public class ChatGUI : MonoBehaviour
{
    public InsightClient insight;

    public InputField nameInput;
    public InputField chatInput;
    public Text textField;

    public void HandleSendChat()
    {
        insight.Send((short)MsgId.Chat, new ChatMsg() { Origin = nameInput.text, Data = chatInput.text });

        chatInput.text = ""; //Clear out the previously entered text from the field
    }
}
