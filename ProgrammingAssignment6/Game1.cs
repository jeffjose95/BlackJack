using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaCards;

namespace ProgrammingAssignment6
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int WindowWidth = 800;
        const int WindowHeight = 600;

       
        
        // max valid blockjuck score for a hand
        const int MaxHandValue = 21;

        // deck and hands
        Deck deck;
        List<Card> dealerHand = new List<Card>();
        List<Card> playerHand = new List<Card>();

        // hand placement
        const int TopCardOffset = 100;
        const int HorizontalCardOffset = 150;
        const int VerticalCardSpacing = 125;

        // messages
        SpriteFont messageFont;
        const string ScoreMessagePrefix = "Score: ";
        Message playerScoreMessage;
        Message dealerScoreMessage;
        Message winnerMessage;
        List<Message> messages = new List<Message>();

        // message placement
        const int ScoreMessageTopOffset = 25;
        const int HorizontalMessageOffset = HorizontalCardOffset;
        Vector2 winnerMessageLocation = new Vector2(WindowWidth / 2,WindowHeight / 2);

        // menu buttons
        Texture2D quitButtonSprite;
        Texture2D hitButtonSprite;
        Texture2D standButtonSprite;
        List<MenuButton> menuButtons = new List<MenuButton>();

        // menu button placement
        const int TopMenuButtonOffset = TopCardOffset;
        const int QuitMenuButtonOffset = WindowHeight - TopCardOffset;
        const int HorizontalMenuButtonOffset = WindowWidth / 2;
        const int VeryicalMenuButtonSpacing = 125;

        // use to detect hand over when player and dealer didn't hit
        bool playerHit = true;
        bool dealerHit = true;

        // game state tracking
        static GameState currentState = GameState.WaitingForPlayer;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set resolution and show mouse
            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;
            IsMouseVisible = true;
            playerHit = true;
            dealerHit = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            
            // create and shuffle deck
            deck = new Deck(Content, 0, 0);
            deck.Shuffle();

            // first player card
            playerHand.Add(deck.TakeTopCard());
            playerHand[0].X = HorizontalCardOffset;
            playerHand[0].Y = TopCardOffset;
           
            if (!playerHand[0].FaceUp)
            {
                playerHand[0].FlipOver();
            }
            
            // first dealer card
            dealerHand.Add(deck.TakeTopCard());
            dealerHand[0].X = HorizontalCardOffset*4;
            dealerHand[0].Y = TopCardOffset;
            if (dealerHand[0].FaceUp)
            {
                dealerHand[0].FlipOver();
            }
            // second player card
            playerHand.Add(deck.TakeTopCard());
            playerHand[1].X = HorizontalCardOffset;
            playerHand[1].Y = TopCardOffset + VerticalCardSpacing;

            if (!playerHand[1].FaceUp)
            {
                playerHand[1].FlipOver();
            }

            // second dealer card
            dealerHand.Add(deck.TakeTopCard());
            dealerHand[1].X = HorizontalCardOffset*4;
            dealerHand[1].Y = TopCardOffset + VerticalCardSpacing;

            if (!dealerHand[1].FaceUp)
            {
                dealerHand[1].FlipOver();
            }

            // load sprite font, create message for player score and add to list
            messageFont = Content.Load<SpriteFont>(@"fonts\Arial24");
            playerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(playerHand).ToString(),
                messageFont,
                new Vector2(HorizontalMessageOffset, ScoreMessageTopOffset));
            messages.Add(playerScoreMessage);

            // load quit button sprite for later use
            quitButtonSprite = Content.Load<Texture2D>(@"graphics\quitbutton");

            // create hit button and add to list
            hitButtonSprite = Content.Load<Texture2D>("graphics//hitButton");
            Vector2 hitCenter = new Vector2(HorizontalCardOffset * 2 + 75, TopCardOffset);
            MenuButton hitButton = new MenuButton(hitButtonSprite, hitCenter,GameState.PlayerHitting);
            menuButtons.Add(hitButton);

            // create stand button and add to list
            standButtonSprite = Content.Load<Texture2D>("graphics//standbutton");
            Vector2 standCenter = new Vector2(HorizontalCardOffset * 2 + 75, TopCardOffset+ VerticalCardSpacing);
            MenuButton standButton = new MenuButton(standButtonSprite, standCenter, GameState.WaitingForDealer);
            menuButtons.Add(standButton);


        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();

            // update menu buttons as appropriate
            foreach (MenuButton thisButton in menuButtons)
            {
                if (currentState == GameState.WaitingForPlayer || currentState == GameState.DisplayingHandResults)
                {
                    if (currentState == GameState.WaitingForPlayer)
                    {
                        playerHit = false;
                    }
                    thisButton.Update(currentMouseState);
                }
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            int newPlayerScore = GetBlockjuckScore(playerHand);
            int newDealerScore = GetBlockjuckScore(dealerHand);

            


            // game state-specific processing
            if ((newDealerScore < MaxHandValue) && (newPlayerScore < MaxHandValue))
            {
                if (currentState == GameState.PlayerHitting)
                {
                    Card thisCard = deck.TakeTopCard();
                    int cardPosition = playerHand.Count;
                    thisCard.X = HorizontalCardOffset;
                    thisCard.Y = TopCardOffset + (VerticalCardSpacing * cardPosition);
                    if (!thisCard.FaceUp)
                    {
                        thisCard.FlipOver();
                    }
                    playerHand.Add(thisCard);

                    newPlayerScore = GetBlockjuckScore(playerHand);
                    playerScoreMessage.Text = ScoreMessagePrefix + newPlayerScore.ToString();
                    playerHit = true;
                    currentState = GameState.WaitingForDealer;

                }

                if (currentState == GameState.WaitingForDealer)
                {
                   
                    if (newPlayerScore > newDealerScore)
                    {
                        currentState = GameState.DealerHitting;
                    }
                    else
                    {
                        dealerHit = false;
                        currentState = GameState.CheckingHandOver;
                    }

                }


                if (currentState == GameState.DealerHitting)
                {
                    Card thisCard = deck.TakeTopCard();
                    int cardPosition = dealerHand.Count;
                    thisCard.X = HorizontalCardOffset * 4;
                    thisCard.Y = TopCardOffset + (VerticalCardSpacing * cardPosition);
                    dealerHand.Add(thisCard);
                    newDealerScore = GetBlockjuckScore(dealerHand);
                    dealerHit = true;
                   
                   
                    currentState = GameState.CheckingHandOver;
                }

                if (currentState == GameState.CheckingHandOver)
                {
                    if (!playerHit && !dealerHit)
                    {
                       
                        dealerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(dealerHand).ToString(), messageFont, new Vector2(HorizontalMessageOffset * 4, ScoreMessageTopOffset));
                        messages.Add(dealerScoreMessage);
                        if (newPlayerScore > newDealerScore)
                        {
                            currentState = GameState.DisplayingHandResults;
                            menuButtons.Clear();
                            winnerMessage = new Message("You Won!", messageFont, winnerMessageLocation);
                            messages.Add(winnerMessage);
                            MenuButton quitButton = new MenuButton(quitButtonSprite, new Vector2(WindowWidth / 2, QuitMenuButtonOffset), GameState.Exiting);
                            menuButtons.Add(quitButton);

                            dealerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(dealerHand).ToString(), messageFont, new Vector2(HorizontalMessageOffset * 4, ScoreMessageTopOffset));
                            messages.Add(dealerScoreMessage);

                        }
                        else if (newPlayerScore == newDealerScore)
                        {
                            currentState = GameState.DisplayingHandResults;
                            menuButtons.Clear();
                            winnerMessage = new Message("Tie", messageFont, winnerMessageLocation);
                            messages.Add(winnerMessage);
                            MenuButton quitButton = new MenuButton(quitButtonSprite, new Vector2(WindowWidth / 2, QuitMenuButtonOffset), GameState.Exiting);
                            menuButtons.Add(quitButton);

                            dealerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(dealerHand).ToString(), messageFont, new Vector2(HorizontalMessageOffset * 4, ScoreMessageTopOffset));
                            messages.Add(dealerScoreMessage);

                        }
                        else
                        {
                            currentState = GameState.DisplayingHandResults;
                            menuButtons.Clear();
                            winnerMessage = new Message("Dealer Won!", messageFont, winnerMessageLocation);
                            messages.Add(winnerMessage);
                            MenuButton quitButton = new MenuButton(quitButtonSprite, new Vector2(WindowWidth / 2, QuitMenuButtonOffset), GameState.Exiting);
                            menuButtons.Add(quitButton);

                            dealerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(dealerHand).ToString(), messageFont, new Vector2(HorizontalMessageOffset * 4, ScoreMessageTopOffset));
                            messages.Add(dealerScoreMessage);

                        }

                        currentState = GameState.DisplayingHandResults;
                    }
                    else
                    {
                        if (newPlayerScore >= MaxHandValue && newDealerScore >= MaxHandValue)
                        {
                            currentState = GameState.DisplayingHandResults;
                            menuButtons.Clear();
                            winnerMessage = new Message("Tie", messageFont, winnerMessageLocation);
                            messages.Add(winnerMessage);
                            MenuButton quitButton = new MenuButton(quitButtonSprite, new Vector2(WindowWidth / 2, QuitMenuButtonOffset), GameState.Exiting);
                            menuButtons.Add(quitButton);

                            dealerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(dealerHand).ToString(), messageFont, new Vector2(HorizontalMessageOffset * 4, ScoreMessageTopOffset));
                            messages.Add(dealerScoreMessage);

                        }
                        else if (newPlayerScore >= MaxHandValue)
                        {
                            currentState = GameState.DisplayingHandResults;
                            menuButtons.Clear();
                            winnerMessage = new Message("Dealer Won!", messageFont, winnerMessageLocation);
                            messages.Add(winnerMessage);
                            MenuButton quitButton = new MenuButton(quitButtonSprite, new Vector2(WindowWidth / 2, QuitMenuButtonOffset), GameState.Exiting);
                            menuButtons.Add(quitButton);

                            dealerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(dealerHand).ToString(), messageFont, new Vector2(HorizontalMessageOffset * 4, ScoreMessageTopOffset));
                            messages.Add(dealerScoreMessage);

                        }
                        else if (newDealerScore >= MaxHandValue)
                        {
                            currentState = GameState.DisplayingHandResults;
                            menuButtons.Clear();
                            winnerMessage = new Message("You Won!", messageFont, winnerMessageLocation);
                            messages.Add(winnerMessage);
                            MenuButton quitButton = new MenuButton(quitButtonSprite, new Vector2(WindowWidth / 2, QuitMenuButtonOffset), GameState.Exiting);
                            menuButtons.Add(quitButton);

                            dealerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(dealerHand).ToString(), messageFont, new Vector2(HorizontalMessageOffset * 4, ScoreMessageTopOffset));
                            messages.Add(dealerScoreMessage);

                        }
                        else
                        {
                            currentState = GameState.WaitingForPlayer;
                        }

                    }
                    foreach (Card thisCard in playerHand)
                    {
                        if (!thisCard.FaceUp)
                        {
                            thisCard.FlipOver();
                        }
                    }
                    foreach (Card thisCard in dealerHand)
                    {
                        if (!thisCard.FaceUp)
                        {
                            thisCard.FlipOver();
                        }
                    }

                

                }
            }
           
            if (currentState == GameState.DisplayingHandResults)
            {
            }

            if (currentState == GameState.Exiting)
            {
                playerHit = true;
                dealerHit = true;
                deck = new Deck(Content, 0, 0);
                playerHand.Clear();
                dealerHand.Clear();
                newPlayerScore = GetBlockjuckScore(playerHand);
                newDealerScore = GetBlockjuckScore(dealerHand);
                messages.Clear();
                menuButtons.Clear();
               
                Vector2 hitCenter = new Vector2(HorizontalCardOffset * 2 + 75, TopCardOffset);
                MenuButton hitButton = new MenuButton(hitButtonSprite, hitCenter, GameState.PlayerHitting);
                menuButtons.Add(hitButton);
                Vector2 standCenter = new Vector2(HorizontalCardOffset * 2 + 75, TopCardOffset + VerticalCardSpacing);
                MenuButton standButton = new MenuButton(standButtonSprite, standCenter, GameState.WaitingForDealer);
                menuButtons.Add(standButton);
                currentState = GameState.WaitingForPlayer;

                deck.Shuffle();

                // first player card
                playerHand.Add(deck.TakeTopCard());
                playerHand[0].X = HorizontalCardOffset;
                playerHand[0].Y = TopCardOffset;

                if (!playerHand[0].FaceUp)
                {
                    playerHand[0].FlipOver();
                }

                // first dealer card
                dealerHand.Add(deck.TakeTopCard());
                dealerHand[0].X = HorizontalCardOffset * 4;
                dealerHand[0].Y = TopCardOffset;
                if (dealerHand[0].FaceUp)
                {
                    dealerHand[0].FlipOver();
                }
                // second player card
                playerHand.Add(deck.TakeTopCard());
                playerHand[1].X = HorizontalCardOffset;
                playerHand[1].Y = TopCardOffset + VerticalCardSpacing;

                if (!playerHand[1].FaceUp)
                {
                    playerHand[1].FlipOver();
                }

                // second dealer card
                dealerHand.Add(deck.TakeTopCard());
                dealerHand[1].X = HorizontalCardOffset * 4;
                dealerHand[1].Y = TopCardOffset + VerticalCardSpacing;

                if (!dealerHand[1].FaceUp)
                {
                    dealerHand[1].FlipOver();
                }
                playerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(playerHand).ToString(), messageFont, new Vector2(HorizontalMessageOffset, ScoreMessageTopOffset));
                messages.Add(playerScoreMessage);
            }


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Goldenrod);

            spriteBatch.Begin();

            // draw hands
            foreach(Card thisCard in playerHand)
            {
                thisCard.Draw(spriteBatch);
            }
            foreach (Card thatCard in dealerHand)
            {
                thatCard.Draw(spriteBatch);
            }

            // draw messages
            foreach (Message thisMessage in messages)
            {
                thisMessage.Draw(spriteBatch);
            }

            // draw menu buttons
            foreach (MenuButton thisButton in menuButtons)
            {
                thisButton.Draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Calculates the Blockjuck score for the given hand
        /// </summary>
        /// <param name="hand">the hand</param>
        /// <returns>the Blockjuck score for the hand</returns>
        private int GetBlockjuckScore(List<Card> hand)
        {
            // add up score excluding Aces
            int numAces = 0;
            int score = 0;
            foreach (Card card in hand)
            {
                if (card.Rank != Rank.Ace)
                {
                    score += GetBlockjuckCardValue(card);
                }
                else
                {
                    numAces++;
                }
            }

            // if more than one ace, only one should ever be counted as 11
            if (numAces > 1)
            {
                // make all but the first ace count as 1
                score += numAces - 1;
                numAces = 1;
            }

            // if there's an Ace, score it the best way possible
            if (numAces > 0)
            {
                if (score + 11 <= MaxHandValue)
                {
                    // counting Ace as 11 doesn't bust
                    score += 11;
                }
                else
                {
                    // count Ace as 1
                    score++;
                }
            }

            return score;
        }

        /// <summary>
        /// Gets the Blockjuck value for the given card
        /// </summary>
        /// <param name="card">the card</param>
        /// <returns>the Blockjuck value for the card</returns>
        private int GetBlockjuckCardValue(Card card)
        {
            switch (card.Rank)
            {
                case Rank.Ace:
                    return 11;
                case Rank.King:
                case Rank.Queen:
                case Rank.Jack:
                case Rank.Ten:
                    return 10;
                case Rank.Nine:
                    return 9;
                case Rank.Eight:
                    return 8;
                case Rank.Seven:
                    return 7;
                case Rank.Six:
                    return 6;
                case Rank.Five:
                    return 5;
                case Rank.Four:
                    return 4;
                case Rank.Three:
                    return 3;
                case Rank.Two:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Changes the state of the game
        /// </summary>
        /// <param name="newState">the new game state</param>
        public static void ChangeState(GameState newState)
        {
            currentState = newState;
        }
    }
}
