# AI-Powered Card Gaming Platform: Where Strategy Meets Innovation

Welcome to the cutting-edge suite of games that combine the power of **AI**, **voice engagement**, and **blockchain technology** to redefine how card games are played. This platform showcases and benchmarks large language models (LLMs) like ChatGPT, Anthropic’s Claude, and Google’s Gemini in real-time gameplay scenarios, offering a unique and immersive experience for players of all skill levels.

---

## **Vision**
This project is more than a collection of games—it’s a vision for what’s possible when AI, blockchain, and interactive gaming converge to create something both innovative and meaningful. By leveraging the Solana blockchain, we ensure **transparency** and **trust** for features like betting and game history while allowing players to:

- Compete against AI models via APIs or local custom endpoints.
- Benchmark AI performance in strategic, real-time scenarios.
- Use voice-to-text engagement for lifelike interactions.
- Learn game strategies, improve their skills, and track progress over time.

---

## **Games in Development**

### **Three Card Brag**
Currently in active development, Three Card Brag combines skill, strategy, and calculated decision-making with the added complexity of real-time AI interactions.

### **other card game Variants**
With classic and innovative twists, Poker is next in line, expanding gameplay into multiplayer setups involving humans and AI players.

These games are just the beginning. The platform will grow to include more games, each designed to:
- Challenge human and AI players alike.
- Leverage blockchain transparency for a fair and immersive experience.
- Offer rich insights into gameplay strategies and decision-making.

### **Cross-Platform Compatibility**

While the current focus is on desktop platforms, such as Linux and Windows, the platform is designed with future expansions in mind. All UI elements and game objects are implemented in 3D, making it straightforward to add support for VR. Although platforms like WebGL face limitations—such as sandbox restrictions that prevent loading local LLMs—these challenges will be addressed using dedicated servers or by leveraging the master player’s desktop as a processing hub for LLM-related tasks. In such setups, non-desktop clients act purely as views, with all critical processing handled by server-authorized components. When no client is desktop-based, the server’s lobby system facilitates the necessary computations.

For WebGL, compatibility is maximized by using UniTask and no threading, ensuring smooth performance across browsers.

To make API integrations flexible, the platform includes features allowing players to contribute to the cost of API tokens or rely on the host when services like OpenAI Azure APIs are chosen. This flexibility ensures broader accessibility while balancing operational costs.

For mobile platforms, where local model loading is constrained by size, lightweight solutions like NanoGPT are being explored. In cases where local models cannot be loaded, mobile platforms will seamlessly rely on the master client or APIs to maintain functionality.

This project is actively under development, with features being built and tested. While progress is ongoing, functionality cannot be guaranteed to work flawlessly at this stage.
Additionally, for real-money gaming (RMG), I am planning a dedicated server lobby system to ensure robust and secure gameplay.


---

## **What Makes This Platform Unique?**

### **AI-Driven Gameplay**
Card games like Poker and Three Card Brag are more than just games of chance. They are skill-based challenges that demand:
- **Strategic thinking**
- **Opponent analysis**
- **Bluffing and calculated decisions**

AI models face unique challenges in this domain, such as analyzing human behavior, adapting to varying playing styles, and making intelligent, real-time decisions under uncertain conditions. To meet these challenges, the platform incorporates multiple specialized agents:

1. **Player History Agent**: Uses blockchain data to analyze playing style and past decisions.
2. **Probability Agent**: Evaluates the current hand and calculates the best moves.
3. **Voice Processing Agent**: Interprets tone, intent, and conversational nuances from real-time voice inputs.
4. **Response Agent**: Crafts lifelike, dynamic voice interactions that mirror human behavior.

### **Voice-to-Text Engagement**
Players can engage with AI opponents using natural voice commands, creating an experience that feels personal and immersive. The AI doesn’t just react to static inputs—it engages, bluffs, and adapts, making each game feel unique.

### **Blockchain Transparency**
By using the Solana blockchain, the platform ensures transparency in:
- Betting and payouts
- Game history and fairness

This foundation builds trust with users, particularly in preparation for future real-money games.

### **Insights and Skill Development**
After each match, players receive detailed game reports that highlight:
- Mistakes made during gameplay
- Areas for improvement
- Actionable strategies for sharpening skills

These insights are invaluable for both casual players and those preparing for competitive play.

---

## **Why Card Games?**
While there are countless card games available, very few combine **AI-driven gameplay** and **blockchain transparency** the way this platform does. Card games provide an ideal medium to:
- Benchmark AI capabilities in strategic and adaptive decision-making.
- Explore the nuances of human-AI interaction through bluffing, strategy, and real-time adaptation.

---

## **A Benchmarking Platform for AI Models**
This platform isn’t just about playing games. It’s about understanding how leading AI models perform in:
- Real-time voice interactions
- Strategic decision-making
- Adaptation to diverse gameplay styles

By comparing models like ChatGPT, Anthropic’s Claude, and Google’s Gemini, the platform offers valuable insights for both players and AI developers.

---

## **Get Involved**
This is just the beginning. Join us as we push the boundaries of what’s possible in gaming, combining **AI**, **voice processing**, and **blockchain** to create an innovative and strategic experience like no other. Whether you’re a casual player, an aspiring card shark, or an AI enthusiast, there’s something here for everyone.

---

### **Contribute to the Project**
I welcome contributions from developers, designers, and AI enthusiasts! This repository is a mirror of my main Plastic SCM repository, which I use as the primary version control system. If you notice large changesets, it's due to the mirroring process.

If you're interested in contributing, let me know, and I can help set up Plastic SCM and provide guidance on what’s needed. For now, this repository serves as a backup and showcase mirror.

---

Let’s redefine what gaming can be.
