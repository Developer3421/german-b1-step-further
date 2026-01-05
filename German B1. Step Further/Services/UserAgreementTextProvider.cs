namespace German_B1._Step_Further.Services
{
    public enum UserAgreementLanguage
    {
        English,
        German,
        Ukrainian
    }

    public sealed class UserAgreementText
    {
        public string WindowTitle { get; init; } = "Угода користувача";
        public string HeaderTitle { get; init; } = "Угода користувача";
        public string Subtitle { get; init; } = "Please read this agreement. To use online AI services you must accept it.";
        public string Body { get; init; } = string.Empty;
        public string Tip { get; init; } = "Tip: you can re-open this dialog from the Instruments window.";
        public string AgreeButton { get; init; } = "I Agree";
        public string DeclineButton { get; init; } = "Decline";
    }

    public static class UserAgreementTextProvider
    {
        public static UserAgreementText Get(UserAgreementLanguage language)
        {
            return language switch
            {
                UserAgreementLanguage.German => German(),
                UserAgreementLanguage.Ukrainian => Ukrainian(),
                _ => English()
            };
        }

        private static UserAgreementText English() => new()
        {
            WindowTitle = "User Agreement",
            HeaderTitle = "User Agreement",
            Subtitle = "Please read this agreement. To use online AI services you must accept it.",
            Body =
                "This application provides learning materials and optional AI tools.\n" +
                "Some AI tools open external web services inside an embedded browser view.\n\n" +
                "1. Local storage (what we store on your device)\n" +
                "The application stores locally only:\n" +
                "• Tab/session information needed to restore your open windows and tabs.\n" +
                "• Whether you have accepted this User Agreement (and the acceptance timestamp).\n" +
                "No other personal data is required by the application.\n\n" +
                "2. Data transmission (what may be sent over the Internet)\n" +
                "The application itself does not automatically send your data anywhere.\n" +
                "Data is transmitted only when you choose to use online tools in the \"Instruments AI\" window\n" +
                "(for example: Google Gemini, Microsoft Copilot, ChatGPT, Perplexity).\n" +
                "When you open an external service, any text you type, paste, upload, or otherwise submit inside that service\n" +
                "is handled by that service provider under their own terms and privacy policies.\n\n" +
                "3. Offline local AI\n" +
                "The offline \"German Chat (Gemma 3)\" feature runs locally on your device without using the Internet.\n\n" +
                "4. Disclaimer\n" +
                "AI-generated content may be incorrect, incomplete, or inappropriate.\n" +
                "Use it as a learning aid and always verify important information.\n\n" +
                "5. Acceptance\n" +
                "By clicking \"I Agree\", you confirm that you understand and accept this agreement.\n" +
                "If you click \"Decline\", online AI services will not open from this application.",
            Tip = "Tip: you can re-open this dialog from the Instruments window.",
            AgreeButton = "I Agree",
            DeclineButton = "Decline"
        };

        private static UserAgreementText German() => new()
        {
            WindowTitle = "Nutzungsvereinbarung",
            HeaderTitle = "Nutzungsvereinbarung",
            Subtitle = "Bitte lesen Sie diese Vereinbarung. Um Online‑KI‑Dienste zu nutzen, müssen Sie zustimmen.",
            Body =
                "Diese Anwendung bietet Lernmaterialien und optionale KI‑Werkzeuge.\n" +
                "Einige KI‑Werkzeuge öffnen externe Web‑Dienste in einer eingebetteten Browseransicht.\n\n" +
                "1. Lokale Speicherung (was wir auf Ihrem Gerät speichern)\n" +
                "Die Anwendung speichert lokal nur:\n" +
                "• Tab-/Sitzungsinformationen zur Wiederherstellung Ihrer geöffneten Fenster und Tabs.\n" +
                "• Ob Sie diese Nutzungsvereinbarung akzeptiert haben (und den Zeitpunkt der Zustimmung).\n" +
                "Es werden keine weiteren personenbezogenen Daten von der Anwendung benötigt.\n\n" +
                "2. Datenübertragung (was über das Internet gesendet werden kann)\n" +
                "Die Anwendung sendet Ihre Daten nicht automatisch.\n" +
                "Daten werden nur übertragen, wenn Sie Online‑Tools im Fenster \"Instruments AI\" verwenden\n" +
                "(z. B. Google Gemini, Microsoft Copilot, ChatGPT, Perplexity).\n" +
                "Wenn Sie einen externen Dienst öffnen, werden Texte/Dateien, die Sie dort eingeben, einfügen oder hochladen,\n" +
                "vom jeweiligen Anbieter gemäß dessen eigenen Nutzungsbedingungen und Datenschutzrichtlinien verarbeitet.\n\n" +
                "3. Offline‑KI\n" +
                "Die Offline‑Funktion \"German Chat (Gemma 3)\" läuft lokal auf Ihrem Gerät ohne Internetverbindung.\n\n" +
                "4. Haftungsausschluss\n" +
                "KI‑generierte Inhalte können falsch, unvollständig oder unangemessen sein.\n" +
                "Nutzen Sie sie als Lernhilfe und prüfen Sie wichtige Informationen immer selbst.\n\n" +
                "5. Zustimmung\n" +
                "Durch Klicken auf \"Ich stimme zu\" bestätigen Sie, dass Sie diese Vereinbarung verstanden und akzeptiert haben.\n" +
                "Wenn Sie auf \"Ablehnen\" klicken, werden Online‑KI‑Dienste in dieser Anwendung nicht geöffnet.",
            Tip = "Tipp: Sie können dieses Dialogfenster im Instruments‑Fenster jederzeit erneut öffnen.",
            AgreeButton = "Ich stimme zu",
            DeclineButton = "Ablehnen"
        };

        private static UserAgreementText Ukrainian() => new()
        {
            WindowTitle = "Угода користувача",
            HeaderTitle = "Угода користувача",
            Subtitle = "Будь ласка, прочитайте цю угоду. Щоб користуватися онлайн AI‑сервісами, потрібно прийняти її.",
            Body =
                "Цей додаток містить навчальні матеріали та опціональні AI‑інструменти.\n" +
                "Деякі AI‑інструменти відкривають зовнішні веб‑сервіси у вбудованому браузері.\n\n" +
                "1. Локальне зберігання (що зберігається на вашому пристрої)\n" +
                "Додаток зберігає локально лише:\n" +
                "• Дані про вкладки/сесії, потрібні для відновлення відкритих вікон і вкладок.\n" +
                "• Факт прийняття цієї Угоди користувача (та час прийняття).\n" +
                "Жодні інші персональні дані додатку не потрібні.\n\n" +
                "2. Передача даних (що може передаватися через Інтернет)\n" +
                "Додаток сам по собі не надсилає ваші дані автоматично.\n" +
                "Дані передаються лише тоді, коли ви свідомо обираєте онлайн інструменти у вікні \"Instruments AI\"\n" +
                "(наприклад: Google Gemini, Microsoft Copilot, ChatGPT, Perplexity).\n" +
                "Коли ви відкриваєте зовнішній сервіс, будь‑який текст або файли, які ви туди вводите/вставляєте/завантажуєте,\n" +
                "обробляються провайдером цього сервісу відповідно до його власних умов та політик конфіденційності.\n\n" +
                "3. Локальний AI без інтернету\n" +
                "Функція \"German Chat (Gemma 3)\" працює локально на вашому пристрої без використання Інтернету.\n\n" +
                "4. Відмова від відповідальності\n" +
                "AI‑згенерований контент може бути неточним, неповним або неприйнятним.\n" +
                "Використовуйте його як допоміжний інструмент навчання та перевіряйте важливу інформацію.\n\n" +
                "5. Прийняття\n" +
                "Натискаючи \"Погоджуюсь\", ви підтверджуєте, що розумієте та приймаєте цю угоду.\n" +
                "Якщо натиснути \"Відхилити\", онлайн AI‑сервіси не будуть відкриватися з цього додатку.",
            Tip = "Підказка: ви можете знову відкрити цю угоду з вікна Instruments.",
            AgreeButton = "Погоджуюсь",
            DeclineButton = "Відхилити"
        };
    }
}
