# enusampler
### 概要
* SimpleEnunuをUTAUのエンジンとして実行します<br>
SimpleEnunu（ https://github.com/oatsu-gh/SimpleEnunu ）

***

### 免責事項
* 本ソフトウェアを使用して生じたいかなる不具合についても、作者は責任を負いません。
* 作者は、本ソフトウェアの不具合を修正する責任を負いません。

***

### 利用方法
Step1
- 同梱されているappsettings.jsonにて各種パスを指定してください(※初回のみ)
```
{
  "Environment": {
    "Python": "",//オプション項目　利用するPython環境をパスで指定できます
    "ENUNU": {
      "Path": "<SimpleEnunuのディレクトリを設定してください>", //例: "G:\SimpleEnunu-0.4.0"
      "TunedWavOut": "false" // 利用する場合はtrueに編集
    }
  }
}
```
- UTAUのプロジェクト設定にてツール2に指定してください。

Step2
- プロジェクトの初回合成については、ustの編集後でないと正常に動作しません。
(理由はtemp$$$.ustを利用することに起因します。)

- 初回合成前には`必ず`ustの編集を行ってください（内容は何でも構いません）

Step3 ※オプション（simpleEnunu0.4.0以前のバージョンでは対応必須）
- simple_enunu040.py,simple_enunu_TunedWavOut.pyのどちらを利用する場合でも、合成後にUTAU上・ENUNU上の両方で再生が始まります。
- 音声がかぶってしまうので、ENUNU側の再生機能をオフにします。
- 下記の通りに`Play_Wav=False`を指定します。
<br>simple_enunu040.py
![image](https://github.com/user-attachments/assets/c2522515-3280-4850-9826-b40ee5aab78e)




