Simple chat demo
================

> A simple JavaScript chat application with Nakama server.

![Simple chat](demo.png?raw=true "Simple chat demo")

The demo uses Semantic UI and React to build a single page component which demonstrates:

* Authentication - Login/register is done with auto-generated device IDs although you should use [email/password or others](https://heroiclabs.com/docs/authentication/) in a real application.
* User accounts - A [user account](https://heroiclabs.com/docs/user-accounts/) is created on registration.
* Realtime chat - Messages sent between many users in a [single chat room](https://heroiclabs.com/docs/social-realtime-chat/#rooms).

## Setup

```
$> yarn install
```

## Run demo

You'll need to download and setup Nakama server. We recommend you use our [Docker compose setup](https://heroiclabs.com/docs/install-docker-quickstart/). With the server and database server setup and running you can run:

```
$> yarn start
```
