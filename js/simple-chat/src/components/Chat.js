import React, { Component } from "react";
import {
  Button,
  Checkbox,
  Comment,
  Container,
  Form,
  Header,
  Input,
  Segment,
  Table
} from "semantic-ui-react";
import { DeviceUUID } from "device-uuid";
import * as nakamajs from "@heroiclabs/nakama-js";

const TOPIC_NAME = "Topic1";
const INTERVAL_PERIOD_MS = 2000;

const errorHandler = error => console.log("Error occurred: %o", error);

export default class Chat extends Component {
  state = {
    host: "127.0.0.1",
    port: "7350",
    serverkey: "defaultkey",
    ssl: false,
    connected: false,

    messages: [],
    message: "",
    autoInterval: null,

    logs: []
  };

  client = null;
  device = Date.now().toString(); // used as a generated device id
  session = null;

  addLogMessage = content => {
    const logs = this.state.logs.concat({
      ts: Date.now(),
      message: content
    });
    this.setState({ logs: logs });
  };

  addMessage = message => {
    const messages = this.state.messages.concat(message);
    this.setState({ messages: messages });
  };

  changeAutoInterval = event => {
    if (this.state.autoInterval) {
      window.clearInterval(this.state.autoInterval);
      this.setState({ autoInterval: null });
      return;
    }
    const intervalId = window.setInterval(() => {
      var message = new nakamajs.TopicMessageSendRequest();
      message.topic = { room: TOPIC_NAME };
      message.data = {
        content: "Sent automated message."
      };
      this.client.send(message).catch(errorHandler);
    }, INTERVAL_PERIOD_MS);
    this.setState({ autoInterval: intervalId });
  };

  changeCheckbox = name => () => this.setState({ [name]: !this.state[name] });

  changeField = name => event => this.setState({ [name]: event.target.value });

  sessionHandler = session => {
    const socket = this.client.createSocket();
    socket
      .connect(session)
      .then(session => {
        this.session = session;
        this.addLogMessage(`New session connected as '${session.id}' user.`);
        this.setState({ connected: true });

        var message = new nakamajs.TopicsJoinRequest();
        message.room(TOPIC_NAME);
        return this.client.send(message);
      })
      .then(result => {
        const topic = result.topics[0];
        this.addLogMessage(
          `User '${topic.self.userId}' joined '${topic.topic.room}' room.`
        );

        var message = new nakamajs.TopicMessageSendRequest();
        message.topic = topic.topic;
        message.data = {
          content: "Hello!"
        };
        return this.client.send(message);
      })
      .catch(errorHandler);
  };

  submitConnect = event => {
    event.preventDefault();

    if (this.client && this.state.connected) {
      this.client.disconnect();
      return;
    }

    this.client = new nakamajs.Client(
      this.state.serverkey,
      this.state.host,
      this.state.port
    );
    this.client.ssl = this.state.useSSL;

    this.client.ondisconnect = event => {
      this.addLogMessage("Disconnected from server.");
      window.clearInterval(this.state.autoInterval);
      this.setState({ connected: false, autoInterval: null });
      this.client = null;
    };

    this.client.ontopicpresence = presence => {
      if (presence.joins) {
        for (let i = 0, l = presence.joins.length; i < l; i++) {
          // convert a presence notification to a chat message for simple view logic.
          const now = `${Date.now()}`;
          this.addMessage({
            createdAt: now,
            messageId: now,
            userId: presence.joins[i].userId,
            data: {
              content: "Joined the room."
            }
          });
        }
      }
      if (presence.leaves) {
        for (let i = 0, l = presence.leaves.length; i < l; i++) {
          // convert a presence notification to a chat message for simple view logic.
          const now = `${Date.now()}`;
          this.addMessage({
            createdAt: now,
            messageId: now,
            userId: presence.leaves[i].userId,
            data: {
              content: "Left the room."
            }
          });
        }
      }
    };

    this.client.ontopicmessage = message => this.addMessage(message);

    const randomUserId = new DeviceUUID().get();

    this.client
      .authenticateDevice({
        id: randomUserId,
        create: true,
        username: "mycustomusername"
      })
      .then(session => {
        console.info("Successfully authenticated:", session).then(session => {
          console.log("session : ", session);
          console.info("Successfully authenticated:", session);
        });
      })
      .catch(error => {
        console.log("error : ", error);
      });

    // this.client
    //   .authenticateCustom({
    //     id: randomUserId,
    //     create: true,
    //     username: "mycustomusername"
    //   })
    //   .then(this.sessionHandler)
    //   .catch(error => {
    //     errorHandler(error);
    //   });
  };

  submitMessage = event => {
    event.preventDefault();

    var message = new nakamajs.TopicMessageSendRequest();
    message.topic = { room: TOPIC_NAME };
    message.data = {
      content: this.state.message
    };
    this.client
      .send(message)
      .then(ack => {
        this.setState({ message: "" });
      })
      .catch(errorHandler);
  };

  render() {
    const messages = [...this.state.messages].sort(
      (a, b) => b.createdAt - a.createdAt
    );
    const logs = [...this.state.logs].sort((a, b) => b.ts - a.ts);
    return (
      <Container fluid>
        <Header size="medium" as="h2">
          Connect settings
        </Header>
        <Form>
          <Form.Group widths="equal">
            <Form.Field
              control={Input}
              label="Host"
              value={this.state.host}
              onChange={this.changeField("host")}
            />
            <Form.Field
              control={Input}
              label="Port"
              value={this.state.port}
              onChange={this.changeField("port")}
            />
            <Form.Field
              control={Input}
              label="Server Key"
              value={this.state.serverkey}
              onChange={this.changeField("serverkey")}
            />
          </Form.Group>
          <Form.Group inline>
            <Form.Field
              control={Checkbox}
              label="Use SSL"
              checked={this.state.ssl}
              onChange={this.changeCheckbox("ssl")}
            />
            <Form.Field
              control={Button}
              color="blue"
              onClick={this.submitConnect}
            >
              {this.state.connected ? "Disconnect" : "Connect"}
            </Form.Field>
          </Form.Group>
        </Form>

        <Header size="medium" as="h2" dividing>
          Messages
        </Header>
        <div
          style={{ height: "180px", marginBottom: "1em", overflowY: "scroll" }}
        >
          <Comment.Group minimal>
            {messages.map(message => (
              <Comment key={message.messageId}>
                <Comment.Content>
                  <Comment.Author>{message.userId}</Comment.Author>
                  <Comment.Metadata>
                    <span>
                      {new Date(message.createdAt * 1).toTimeString()}
                    </span>
                  </Comment.Metadata>
                  <Comment.Text>
                    <p>{message.data.content}</p>
                  </Comment.Text>
                </Comment.Content>
              </Comment>
            ))}
          </Comment.Group>
        </div>

        <Form reply>
          <Form.Group inline>
            <Form.Field
              control={Input}
              placeholder="Message"
              width="8"
              value={this.state.message}
              disabled={!this.state.connected}
              onChange={this.changeField("message")}
            />
            <Form.Field
              control={() => (
                <Button
                  content="Send"
                  labelPosition="left"
                  icon="edit"
                  color="blue"
                  disabled={
                    !this.state.connected || this.state.message.length < 1
                  }
                  onClick={this.submitMessage}
                />
              )}
            />
            <Form.Button
              color="blue"
              onClick={() => this.setState({ messages: [] })}
            >
              Clear
            </Form.Button>
            <Form.Field
              control={Checkbox}
              label="Auto Message"
              disabled={!this.state.connected}
              checked={!!this.state.autoInterval}
              onClick={this.changeAutoInterval}
            />
          </Form.Group>
        </Form>

        <Header size="medium" as="h2">
          Logs
        </Header>
        <Table compact singleLine striped attached="top">
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell width="5">Date</Table.HeaderCell>
              <Table.HeaderCell width="11">Message</Table.HeaderCell>
            </Table.Row>
          </Table.Header>
        </Table>
        <Segment
          attached="bottom"
          style={{ padding: "0", height: "150px", overflowY: "scroll" }}
        >
          <Table compact singleLine striped style={{ border: "0" }}>
            <Table.Body>
              {logs.map(log => (
                <Table.Row key={log.ts}>
                  <Table.Cell width="5">
                    {new Date(log.ts).toTimeString()}
                  </Table.Cell>
                  <Table.Cell width="11">{log.message}</Table.Cell>
                </Table.Row>
              ))}
            </Table.Body>
          </Table>
        </Segment>
      </Container>
    );
  }
}
