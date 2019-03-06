# スマートスピーカーを遊びたおす会で使用したデモアプリ

Durable Functions で時間のかかる処理を動かしつつ、スマートスピーカーで進捗確認が出来るサンプルです。

## 動かし方

### Azure 側

- Azure ポータルで Function App を作成します。
- LongRunningProcessSkill を作成した Function App に発行します。

### スマートスピーカー

LINE Developers, Dialogflow, Alexa Skill Kit でスキルを作ります。
インテントは「進捗どうですか」という単語に反応する CheckStatusIntent を 1 つ作成します。

デプロイした LongRunningProcessSkill を Azure ポータルで見ると Dialogflow, Alexa, Clova という関数があるので、自分の使用しているプラットフォームの名前の関数の URL を取得して Webhook として呼ばれるように URL を各プラットフォームの設定方法に従って設定してください。

### クライアント側

時間のかかる処理にクライアントサイドでボタンを押すという処理があるので、動くように構成します。

このプログラムの動作環境は Windows 10 である必要があります。

```
処理コードを参考にして .NET Core で実装しなおせば他のプラットフォームでも動きます。
```

LongRunningProcessSkill.UWP の `Secrets_Sample.cs` を開いてクラス名を `Secrets` に変更します。

ファイルの中に書いてある 3 つの値を設定してビルドして実行できれば準備は完了です。

## サンプルの動作

- スキル起動後、最初の挨拶が走ります
  - 前に実行途中の処理などがあれば、その内容を説明します。
  - 処理が走ってなければ新しい処理が実行されます。
- クラウド側で 15 秒かけて「こんにちは、ちょまどさん」というメッセージを生成します
- 続けてクラウド側でユーザーが LongRunningProcessSkill.UWP でボタンを押すまで待機します。
- LongRunningProcessSkill.UWP でリフレッシュボタンを押すと現在処理が必要なものの一覧が出ます
- 一覧から１項目選択して任意のテキストを入れて送信するとクラウド側の処理が先に進みます。
- クラウド側で 15 秒かけて最終的なメッセージを生成します。
- 処理が完了した状態でスマートスピーカーに「進捗どうですか」と聞くと処理結果のメッセージと、処理にかかったトータルの時間を話します。


## 注意事項 1

廃止が予定されているユーザー ID に依存しています。

本番では、何かしらのサインイン機能を実装してユーザーを識別する情報はそこからとってくる必要があります。

## 注意事項 2

一応 Clova, Google Assistant, Alexa 対応しているつもりですが個人のスマートスピーカーの所有事情により Google Assitatnt のエミュレーターと Google Home による実機動作確認しかしていません。

Clova と Alexa は動かない可能性があります。
動かなかった場合は Pull Request お待ちしております。
