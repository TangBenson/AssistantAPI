import './App.css';
import React, { useState, useEffect } from "react";
import "@chatscope/chat-ui-kit-styles/dist/default/styles.min.css"
import { MainContainer, ChatContainer, MessageList, Message, MessageInput, TypingIndicator } from "@chatscope/chat-ui-kit-react";
import picc from './cat.png';

function App() {
  const [initialized, setInitialized] = useState(false);
  const [threadId, setThreadId] = useState(null);
  //true: GPT正在回覆訊息
  const [typing, setTyping] = useState(false);
  const [messageArray, setMessages] = useState([
    {
      message: "還迎來到八卦天地，專門提供沒營養的新聞"
      // image: data.image
    }
  ]);

  useEffect(() => {
    if (!initialized) {
      createNewThread();
      setInitialized(true);
    }
  });

  const createNewThread = async () => {
    await fetch("https://localhost:9001/api/AIWeather/CreateThreadEndpoint")
    .then(response => response.text())
    .then((data) => {
      console.log(data);
      setThreadId(data);
      console.log(threadId);
    })
  };

  //按下發送訊息按鈕
  const sendQuery = async (message) => {
    const newMessage = {
      message: message
      // image: data.image
    }
    //newMessage加到已存在的messageArray陣列，結果存到newMessageArray，但messageArray不會變
    const newMessageArray = [...messageArray, newMessage];
    setMessages(newMessageArray);
    setTyping(true);
    await processMessage(newMessageArray);
  };

  //發送訊息給後端
  async function processMessage(messageArray) {
    if (!threadId) {
      console.error("Thread ID is not set");
      return;
    }
    let message = messageArray[messageArray.length - 1];
    console.log(message);
    let chatRequest = {
        Msg: message.message,
        ThreadId: threadId
    }
    await fetch("https://localhost:9001/api/AIWeather/ChatEndpoint", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(chatRequest),
    })
    .then(response => response.text()) // 或者 response.json()，如果回傳的是 JSON
    .then(data => {
      console.log(data); // 這裡的 data 就是你的字串或者 JSON 物件
      setMessages([
        ...messageArray, {
          message: data
          // image: data.image
        }
      ]);
      setTyping(false);
    })
  };

  return (
    <div className="flex flex-col h-screen items-center px-4">
      <h1 className="mt-8 mb-4 text-3xl font-mono font-semibold">狗仔隊</h1>
      <img src={picc} alt="Curious Black Cat" width={100} height={100} className="mb-4 rounded-full shadow-lg border-2" />
      <div className="w-1/2 h-screen mb-8">
        <MainContainer className="rounded-lg shadow-lg max-h-[45rem]">
          <ChatContainer>
            <MessageList
              className="my-4"
              scrollBehavior="smooth"
              typingIndicator={typing ? <TypingIndicator content="AI正在答覆你的問題..." /> : null}
            >
              {messageArray.map((message, i) => {
                return (
                  <Message key={i} model={message}>
                    {/* {message.image && <Message.ImageContent src={message.image} alt="Image" width={500} />} */}
                  </Message>
                );
              })}
            </MessageList>
            <MessageInput placeholder="問點啥~" onSend={sendQuery} />
          </ChatContainer>
        </MainContainer>
      </div>
    </div>
  );
}

export default App;
