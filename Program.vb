'Imports System
'Imports System.Linq ' Used for the OrderBy and GroupBy queries

Namespace Poker
	Friend Class Program
		' Card faces, in ascending order of value
		Public Enum CardFace
			Two
			Three
			Four
			Five
			Six
			Seven
			Eight
			Nine
			Ten
			Jack
			Queen
			King
			Ace
		End Enum

		' Card suits, in alpha order
		Public Enum CardSuit
			Club
			Diamond
			Heart
			Spade
		End Enum

		' Game stages
		Public Enum GameStage
			Deal
			Draw
			Score
			[End]
		End Enum

		' Hand ranks, in ascending order of rank
		Public Enum HandRank
			Unranked
			[Nothing]
			Pair
			TwoPair
			ThreeOfAKind
			Straight
			Flush
			FullHouse
			FourOfAKind
			StraightFlush
			RoyalFlush
		End Enum

		Public Class Card ' Definition for a card
			' Holds card face
			Public face As CardFace

			' Holds card suit
			Public suit As CardSuit

			' Class constructor that takes a card face and suit
			Public Sub New(ByVal cardFace As CardFace, ByVal cardSuit As CardSuit)
				face = cardFace
				suit = cardSuit
			End Sub
		End Class

		Public Class CardDeck ' Definition for a deck of cards (52 card deck - no jokers)
			' Holds collection of cards
			Public cards As New List(Of Card)()

			' Constructor - creates the initial deck
			Public Sub New()
				' Deck is created in order of suit, then face value
				For Each suit As CardSuit In System.Enum.GetValues(GetType(CardSuit))
					For Each face As CardFace In System.Enum.GetValues(GetType(CardFace))
						Dim newCard As New Card(face, suit)
						cards.Add(newCard)
					Next face
				Next suit
			End Sub

			' Shuffles the deck a single time
			Public Sub Shuffle()
				Dim random As New Random()
				Dim shuffledCards As New List(Of Card)()
				' Loop until the new shuffled cards collection has 52 cards
				Do While shuffledCards.Count < 52
					' Get random card number from 0-51 (cards collection is zero-based)
					Dim randomCardNum As Integer = random.Next(0, 52)
					Dim randomCard As Card = cards(randomCardNum)
					' Only add this randomly chosen card if it is not already in the shuffled deck
					If Not shuffledCards.Contains(randomCard) Then
						shuffledCards.Add(randomCard)
					End If
				Loop
				' Replace original cards with these shuffled cards
				cards = shuffledCards
			End Sub

			' Shuffles the deck a specified number of times
			Public Sub Shuffle(ByVal numTimes As Integer)
				For i As Integer = 1 To numTimes
					Shuffle() ' Call single shuffle
				Next i
			End Sub

			' Deal single card from top of deck to specified hand
			Public Function DealCard(ByVal hand As Hand) As Hand
				' Cards are dealt from the top of the deck
				Dim dealtCard As Card = cards(0)
				hand.cards.Add(dealtCard)
				' Remove top card from deck since it is now in player hand
				cards.RemoveAt(0)
				Return hand
			End Function

			' Draw single card from top of deck and replace specified card (number) in specified hand
'INSTANT VB NOTE: The parameter drawCard was renamed since Visual Basic will not allow parameters with the same name as their enclosing function or property:
			Public Function DrawCard(ByVal hand As Hand, ByVal drawCard_Renamed As Integer) As Hand
				' Cards are dealt from the top of the deck, original card is discarded
				Dim dealtCard As Card = cards(0)
				' Discard original card in hand (zero-based)
				hand.cards.RemoveAt(drawCard_Renamed-1)
				' Insert new drawn card in hand at the same location
				hand.cards.Insert(drawCard_Renamed-1, dealtCard)
				' Remove top card from deck since it is now in player hand
				cards.RemoveAt(0)
				Return hand
			End Function
		End Class

		Public Class Hand ' Definition for a card hand
			' Holds collection of cards
			Public cards As New List(Of Card)()

			' Holds hand rank - starts with being unranked
			Public rank As HandRank = HandRank.Unranked

			' Holds face value of high card in rank
			Public highCard As CardFace

			' Holds the player number associated with this hand
			Public playerNumber As Integer

			' Returns multiple lines as a string with the card number and face value plus suit
			Public Function DisplayHand() As String
				Dim returnHand As String = ""
				' Loop through collection of cards in hand (zero-based)
				For cardCount As Integer = 0 To cards.Count - 1
					returnHand &= "Card " & (cardCount+1).ToString() & ": " & cards(cardCount).face.ToString() & " of " & cards(cardCount).suit.ToString() & "s" & vbLf
				Next cardCount
				Return returnHand
			End Function

			' Private method to check rank - "Pair"
			Private Sub CheckPair()
				' Group cards by face value
				Dim tmpCards = cards.GroupBy(Function(Card) Card.face)
				For Each faceGroup In tmpCards
					If faceGroup.Count()>=2 Then ' Look for a count of 2 per face
						rank = HandRank.Pair
						highCard = faceGroup.Key ' Face value of pair
					End If
				Next faceGroup
			End Sub

			' Private method to check rank - "Two Pair"
			Private Sub CheckTwoPair()
				' Group cards by face value
				Dim tmpCards = cards.GroupBy(Function(Card) Card.face)
				' Holds the number of pairs that are found
				Dim numPairs As Integer = 0
				' Holds the highest card found with a pair
				Dim tempHigh As CardFace = CardFace.Two
				For Each faceGroup In tmpCards
					If faceGroup.Count()>=2 Then ' Look for a count of 2 per face
						numPairs += 1
						If faceGroup.Key >= tempHigh Then
							tempHigh = faceGroup.Key ' Set new high card
						End If
					End If
				Next faceGroup
				If numPairs>=2 Then ' Only set rank if we find 2 pairs
					rank = HandRank.TwoPair
					highCard = tempHigh
				End If
			End Sub

			' Private method to check rank - "Three of a Kind"
			Private Sub CheckThreeOfAKind()
				' Group cards by face value
				Dim tmpCards = cards.GroupBy(Function(Card) Card.face)
				For Each faceGroup In tmpCards
					If faceGroup.Count()>=3 Then ' Look for a count of 3 per face
						rank = HandRank.ThreeOfAKind
						highCard = faceGroup.Key
					End If
				Next faceGroup
			End Sub

			' Private method to check rank - "Straight"
			Private Sub CheckStraight()
				' Holds the total difference in values between cards (cards are already sorted by value)
				Dim diffCount As Integer = 0
				' Calculate total difference in card values between cards
				For numCard As Integer = 2 To 5
					diffCount += cards(numCard-1).face - cards(numCard-2).face
				Next numCard
				If diffCount = 4 Then ' Each card is different by a value of 1
					rank = HandRank.Straight
				End If
				If diffCount = 12 AndAlso cards(4).face = CardFace.Ace AndAlso cards(0).face = CardFace.Two Then ' Special case for Ace at the start of a straight
					rank = HandRank.Straight
				End If
				If rank = HandRank.Straight Then
					highCard = cards(4).face ' Set high card
				End If
			End Sub

			' Private method to check rank - "Flush"
			Private Sub CheckFlush()
				' Group cards by suit value
				Dim tmpCards = cards.GroupBy(Function(Card) Card.suit)
				For Each suitGroup In tmpCards
					If suitGroup.Count()>=5 Then ' Look for a single grouping of all 5 cards
						rank = HandRank.Flush
					End If
				Next suitGroup
				If rank = HandRank.Flush Then
					highCard = cards(4).face ' Set high card
				End If
			End Sub

			' Private method to check rank - "Full House"
			Private Sub CheckFullHouse()
				' Group cards by face value
				Dim tmpCards = cards.GroupBy(Function(Card) Card.face)
				' Holds the number of two pair
				Dim numTwoPair As Integer = 0
				' Holds the number of three pair
				Dim numThreePair As Integer = 0
				' Holds the highest card found with a pair
				Dim tempHigh As CardFace = CardFace.Two
				For Each faceGroup In tmpCards
					If faceGroup.Count()=2 Then ' Look for a two pair
						numTwoPair += 1
					End If
					If faceGroup.Count()=3 Then ' Look for a three pair (use the three pair for high card)
						numThreePair += 1
						If faceGroup.Key >= tempHigh Then
							tempHigh = faceGroup.Key
						End If
					End If
				Next faceGroup
				If numTwoPair=1 AndAlso numThreePair=1 Then ' Only set rank if we find a 2 and a 3 pair
					rank = HandRank.FullHouse
					highCard = tempHigh
				End If
			End Sub

			' Private method to check rank - "Four of a Kind"
			Private Sub CheckFourOfAKind()
				' Group cards by face value
				Dim tmpCards = cards.GroupBy(Function(Card) Card.face)
				For Each faceGroup In tmpCards
					If faceGroup.Count()>=4 Then ' Look for a count of 4 per face
						rank = HandRank.FourOfAKind
						highCard = faceGroup.Key
					End If
				Next faceGroup
			End Sub

			' Private method to check rank - "Straight Flush" or "Royal Flush"
			Private Sub CheckStraightFlush()
				' First check to see if this is a flush at all
				CheckFlush()
				If rank = HandRank.Flush Then ' If at least flush, check for straight or royal
					' Holds the total difference in values between cards (cards are already sorted by value)
					Dim diffCount As Integer = 0
					' Calculate total difference in card values between cards
					For numCard As Integer = 2 To 5
						diffCount += cards(numCard-1).face - cards(numCard-2).face
					Next numCard
					If diffCount = 12 AndAlso cards(4).face = CardFace.Ace AndAlso cards(0).face = CardFace.Two Then ' Special case for Ace at the start of a straight
						rank = HandRank.StraightFlush
					End If
					If diffCount = 4 AndAlso cards(4).face <> CardFace.Ace Then ' Standard straight flush
						rank = HandRank.StraightFlush
					End If
					If diffCount = 4 AndAlso cards(4).face = CardFace.Ace Then ' Ace at the end of the straight makes it a royal flush
						rank = HandRank.RoyalFlush
					End If
					If rank = HandRank.StraightFlush OrElse rank = HandRank.RoyalFlush Then
						highCard = cards(4).face ' Set high card
					End If
				End If
			End Sub

			' Private method to sort cards based on face value
			Private Sub SortCards()
				cards.Sort(Function(x, y) x.face.CompareTo(y.face))
			End Sub

			' Calculates the rank of the hand
			' First cards are sorted by face value, then each rank is "checked" in descending
			' order of rank (and it making a rank will keep it from any further checks)
			Public Sub GetRank()
				' Sort cards by face value
				SortCards()

				If rank = HandRank.Unranked Then
					CheckStraightFlush()
				End If

				If rank = HandRank.Unranked Then
					CheckFourOfAKind()
				End If

				If rank = HandRank.Unranked Then
					CheckFullHouse()
				End If

				If rank = HandRank.Unranked Then
					CheckFlush()
				End If

				If rank = HandRank.Unranked Then
					CheckStraight()
				End If

				If rank = HandRank.Unranked Then
					CheckThreeOfAKind()
				End If

				If rank = HandRank.Unranked Then
					CheckTwoPair()
				End If

				If rank = HandRank.Unranked Then
					CheckPair()
				End If

				If rank = HandRank.Unranked Then ' If no rank is valud, then rank is "Nothing" and we just store the high card
					rank = HandRank.Nothing
					highCard = cards(4).face ' Set high card
				End If

			End Sub
		End Class

		' Returns the number of players based on input
		Private Shared Function GetPlayers() As Integer
			' Holds the input as a string
			Dim getInput As String
			' Holds the input coverted to an int
			Dim inputResult As Integer = Nothing
			' Used by loop to continue to prompt until proper input
			Dim valid As Boolean = False
			Do
				Console.Write("Please input number of players (2-7): ")
				' Get user input
				getInput = Console.ReadLine()
				Console.WriteLine("")
				If Integer.TryParse(getInput, inputResult) Then ' Check to see if input can be parsed as an int
					If inputResult>=2 AndAlso inputResult<=7 Then ' Only allow an int between 2 and 7
						valid = True
					End If
				End If
			Loop While Not valid
			Return inputResult
		End Function

		Private Shared Function GetComputerPlayer() As Boolean
			Return True
		End Function

		' Returns a collection of ints that represent the cards chosen from players
		' hand for the draw
		Private Shared Function GetDrawCards() As List(Of Integer)
			' Holds input as a string
			Dim getInput As String
			' Holds collection of ints to be returned (start empty)
			Dim returnDraw As New List(Of Integer)()
			' Get input
			getInput = Console.ReadLine()
			' Convert input to array of strings, split using ',' character
			Dim inputItems() As String = getInput.Split(","c)
			' Loop through array results
			For numItem As Integer = 0 To inputItems.Length - 1
				' Holds int value of element in the array
				Dim itemVal As Integer = Nothing
				If Integer.TryParse(inputItems(numItem).Trim(), itemVal) Then ' Check to see if string can be parsed as an int
					If returnDraw.Count<5 Then ' Only allow a maximum of 5 cards in draw - others are ignored
						returnDraw.Add(itemVal)
					End If
				End If
			Next numItem
			Return returnDraw
		End Function

		Shared Sub Main(ByVal args() As String)
			Console.WriteLine("Welcome to Poker!" & vbLf)

			' Get number of players
			Dim numPlayers As Integer = GetPlayers()

			' Get computer as player or not
			Dim computerPlayer As Boolean = GetComputerPlayer()

			' Create deck and shuffle it (3 times)
			Dim deck As New CardDeck()
			deck.Shuffle(3)

			' Tracks stage of game - Deal, Draw, Score and End
			Dim gameStage As GameStage = Poker.Program.GameStage.Deal

			' Create a collection of hands to represent each player
			Dim hands As New List(Of Hand)()
			For playerHands As Integer = 1 To numPlayers
				Dim newHand As New Hand()
				newHand.playerNumber = playerHands ' Holds the player number - used at scoring since these will be sorted
				hands.Add(newHand)
			Next playerHands

			' Main game loop
			Do
				Select Case gameStage
					Case Poker.Program.GameStage.Deal ' Deal cards to players
						' Card loop - 5 cards
						For numCards As Integer = 1 To 5
							' Player loop
							For numPlayer As Integer = 1 To numPlayers
								' Hands collection is zero-based
								hands(numPlayer-1) = deck.DealCard(hands(numPlayer-1))
							Next numPlayer
						Next numCards
						Console.WriteLine(vbLf & "All hands are dealt" & vbLf)
						' Display hands for each player
						For numPlayer As Integer = 1 To numPlayers
							Console.WriteLine("Player " & numPlayer.ToString() & " hand:")
							Console.WriteLine(hands(numPlayer-1).DisplayHand())
						Next numPlayer
						' Change game stage
						gameStage = Poker.Program.GameStage.Draw
					Case Poker.Program.GameStage.Draw ' Allow each player to draw cards
						Console.WriteLine(vbLf & "Now time to choose draw" & vbLf)
						' Redisplay hand for each player, then get input on which cards are part of the
						' draw - input is card numbers seperated by commas
						For numPlayer As Integer = 1 To numPlayers
							Console.WriteLine("Player " & numPlayer.ToString() & " hand:")
							Console.WriteLine(hands(numPlayer-1).DisplayHand())
							Console.WriteLine(vbLf & "Enter the cards you would like to use in the draw")
							Console.Write("(card numbers seperated by commas, hit enter for none): ")
							' Gets a collection of ints representing the card numbers in the draw (zero-based)
							Dim drawCards As List(Of Integer) = GetDrawCards()
							For drawCard As Integer = 0 To drawCards.Count - 1
								' Only do draw if the input card is a valid card number (1-5)
								If drawCards(drawCard)>=1 AndAlso drawCards(drawCard)<=5 Then
									hands(numPlayer-1) = deck.DrawCard(hands(numPlayer-1), drawCards(drawCard))
								End If
							Next drawCard
							Console.WriteLine("")
						Next numPlayer
						' Change game stage
						gameStage = Poker.Program.GameStage.Score
					Case Poker.Program.GameStage.Score ' Calculate score by ranking each hand for players
						Console.WriteLine(vbLf & "Final hands of players" & vbLf)
						' Redisplay hand for each player so that they see the results of the previous draw
						' and can see both the rank of the hand and what the "high" card was for that rank
						For numPlayer As Integer = 1 To numPlayers
							hands(numPlayer-1).GetRank()
							Console.WriteLine("Player " & numPlayer.ToString() & " hand:")
							Console.WriteLine(hands(numPlayer-1).DisplayHand())
							Console.WriteLine("High Card: " & hands(numPlayer-1).highCard.ToString())
							Console.WriteLine("RANK: " & hands(numPlayer-1).rank.ToString() & vbLf)
						Next numPlayer
						' Order hands based on descending rank, then desending high card, then player number
						' This means there are no ties - a tie is broken by the player number (should it be?)
						Dim orderedHands As List(Of Hand) = hands.OrderByDescending(Function(x) x.rank).ThenByDescending(Function(x) x.highCard).ThenBy(Function(x) x.playerNumber).ToList()
						' Display winner player and that player's hand rank and high card
						Console.WriteLine("Winner is player " & orderedHands(0).playerNumber.ToString())
						Console.WriteLine("with a rank of: " & orderedHands(0).rank.ToString() & ", high card: " & orderedHands(0).highCard.ToString())
						' Change game stage
						gameStage = Poker.Program.GameStage.End
				End Select
			Loop While gameStage <> Poker.Program.GameStage.End

            ' End of game
            Console.WriteLine(vbLf & "Thanks for playing!")

            If (System.Diagnostics.Debugger.IsAttached) Then
                Console.WriteLine(vbLf & "Hit any key to quit")
                Console.ReadKey()
            End If

        End Sub
	End Class
End Namespace
