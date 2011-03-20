﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.832
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=2.0.50727.42.
// 
namespace HandHistory {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public partial class PokerHandXML {
        
        private PokerHand[] handsField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Hands")]
        public PokerHand[] Hands {
            get {
                return this.handsField;
            }
            set {
                this.handsField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PokerHand {
        
        private Blind[] blindsField;
        
        private Card[] holeCardsField;
        
        private Round[] roundsField;
        
        private Context contextField;
        
        private HandResult[] resultsField;
        
        private Player[] playersField;
        
        private decimal rakeField;
        
        private string heroField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Blinds")]
        public Blind[] Blinds {
            get {
                return this.blindsField;
            }
            set {
                this.blindsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("HoleCards")]
        public Card[] HoleCards {
            get {
                return this.holeCardsField;
            }
            set {
                this.holeCardsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Rounds")]
        public Round[] Rounds {
            get {
                return this.roundsField;
            }
            set {
                this.roundsField = value;
            }
        }
        
        /// <remarks/>
        public Context Context {
            get {
                return this.contextField;
            }
            set {
                this.contextField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Results")]
        public HandResult[] Results {
            get {
                return this.resultsField;
            }
            set {
                this.resultsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Players")]
        public Player[] Players {
            get {
                return this.playersField;
            }
            set {
                this.playersField = value;
            }
        }
        
        /// <remarks/>
        public decimal Rake {
            get {
                return this.rakeField;
            }
            set {
                this.rakeField = value;
            }
        }
        
        /// <remarks/>
        public string Hero {
            get {
                return this.heroField;
            }
            set {
                this.heroField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Blind {
        
        private string playerField;
        
        private BlindType typeField;
        
        private decimal amountField;
        
        private bool allInField;
        
        public Blind() {
            this.typeField = BlindType.None;
            this.amountField = ((decimal)(0.00m));
            this.allInField = false;
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Player {
            get {
                return this.playerField;
            }
            set {
                this.playerField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(BlindType.None)]
        public BlindType Type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(typeof(decimal), "0.00")]
        public decimal Amount {
            get {
                return this.amountField;
            }
            set {
                this.amountField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool AllIn {
            get {
                return this.allInField;
            }
            set {
                this.allInField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    public enum BlindType {
        
        /// <remarks/>
        None,
        
        /// <remarks/>
        Ante,
        
        /// <remarks/>
        SmallBlind,
        
        /// <remarks/>
        BigBlind,
        
        /// <remarks/>
        LateBlind,
        
        /// <remarks/>
        DeadBlind,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Player {
        
        private string nameField;
        
        private decimal stackField;
        
        private int seatField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Stack {
            get {
                return this.stackField;
            }
            set {
                this.stackField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Seat {
            get {
                return this.seatField;
            }
            set {
                this.seatField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Pot {
        
        private decimal amountField;
        
        private int numberField;
        
        public Pot() {
            this.amountField = ((decimal)(0.00m));
            this.numberField = 0;
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(typeof(decimal), "0.00")]
        public decimal Amount {
            get {
                return this.amountField;
            }
            set {
                this.amountField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int Number {
            get {
                return this.numberField;
            }
            set {
                this.numberField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class HandResult {
        
        private Card[] holeCardsField;
        
        private Pot[] wonPotsField;
        
        private string playerField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("HoleCards")]
        public Card[] HoleCards {
            get {
                return this.holeCardsField;
            }
            set {
                this.holeCardsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("WonPots")]
        public Pot[] WonPots {
            get {
                return this.wonPotsField;
            }
            set {
                this.wonPotsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Player {
            get {
                return this.playerField;
            }
            set {
                this.playerField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Card {
        
        private Rank rankField;
        
        private Suit suitField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Rank Rank {
            get {
                return this.rankField;
            }
            set {
                this.rankField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Suit Suit {
            get {
                return this.suitField;
            }
            set {
                this.suitField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    public enum Rank {
        
        /// <remarks/>
        None,
        
        /// <remarks/>
        Two,
        
        /// <remarks/>
        Three,
        
        /// <remarks/>
        Four,
        
        /// <remarks/>
        Five,
        
        /// <remarks/>
        Six,
        
        /// <remarks/>
        Seven,
        
        /// <remarks/>
        Eight,
        
        /// <remarks/>
        Nine,
        
        /// <remarks/>
        Ten,
        
        /// <remarks/>
        Jack,
        
        /// <remarks/>
        Queen,
        
        /// <remarks/>
        King,
        
        /// <remarks/>
        Ace,
        
        /// <remarks/>
        Joker,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    public enum Suit {
        
        /// <remarks/>
        None,
        
        /// <remarks/>
        Clubs,
        
        /// <remarks/>
        Diamonds,
        
        /// <remarks/>
        Hearts,
        
        /// <remarks/>
        Spades,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Context {
        
        private bool onlineField;
        
        private string siteField;
        
        private string idField;
        
        private string tableField;
        
        private System.DateTime timeStampField;
        
        private GameFormat formatField;
        
        private int buttonField;
        
        private decimal bigBlindField;
        
        private decimal smallBlindField;
        
        private decimal anteField;
        
        private BettingType bettingTypeField;
        
        private bool cappedField;
        
        private decimal capAmountField;
        
        private bool capAmountFieldSpecified;
        
        private PokerVariant pokerVariantField;
        
        public Context() {
            this.onlineField = true;
            this.anteField = ((decimal)(0.00m));
            this.cappedField = false;
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool Online {
            get {
                return this.onlineField;
            }
            set {
                this.onlineField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Site {
            get {
                return this.siteField;
            }
            set {
                this.siteField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="ID")]
        public string ID {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Table {
            get {
                return this.tableField;
            }
            set {
                this.tableField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime TimeStamp {
            get {
                return this.timeStampField;
            }
            set {
                this.timeStampField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public GameFormat Format {
            get {
                return this.formatField;
            }
            set {
                this.formatField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Button {
            get {
                return this.buttonField;
            }
            set {
                this.buttonField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal BigBlind {
            get {
                return this.bigBlindField;
            }
            set {
                this.bigBlindField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal SmallBlind {
            get {
                return this.smallBlindField;
            }
            set {
                this.smallBlindField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(typeof(decimal), "0.00")]
        public decimal Ante {
            get {
                return this.anteField;
            }
            set {
                this.anteField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public BettingType BettingType {
            get {
                return this.bettingTypeField;
            }
            set {
                this.bettingTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Capped {
            get {
                return this.cappedField;
            }
            set {
                this.cappedField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal CapAmount {
            get {
                return this.capAmountField;
            }
            set {
                this.capAmountField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CapAmountSpecified {
            get {
                return this.capAmountFieldSpecified;
            }
            set {
                this.capAmountFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PokerVariant PokerVariant {
            get {
                return this.pokerVariantField;
            }
            set {
                this.pokerVariantField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    public enum GameFormat {
        
        /// <remarks/>
        CashGame,
        
        /// <remarks/>
        SitNGo,
        
        /// <remarks/>
        MultiTableTournament,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    public enum BettingType {
        
        /// <remarks/>
        FixedLimit,
        
        /// <remarks/>
        PotLimit,
        
        /// <remarks/>
        NoLimit,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    public enum PokerVariant {
        
        /// <remarks/>
        TexasHoldEm,
        
        /// <remarks/>
        OmahaHi,
        
        /// <remarks/>
        OmahaHiLo,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Action {
        
        private string playerField;
        
        private ActionType typeField;
        
        private decimal amountField;
        
        private bool allInField;
        
        public Action() {
            this.typeField = ActionType.None;
            this.amountField = ((decimal)(0.00m));
            this.allInField = false;
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Player {
            get {
                return this.playerField;
            }
            set {
                this.playerField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(ActionType.None)]
        public ActionType Type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(typeof(decimal), "0.00")]
        public decimal Amount {
            get {
                return this.amountField;
            }
            set {
                this.amountField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool AllIn {
            get {
                return this.allInField;
            }
            set {
                this.allInField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    public enum ActionType {
        
        /// <remarks/>
        None,
        
        /// <remarks/>
        Fold,
        
        /// <remarks/>
        Check,
        
        /// <remarks/>
        Call,
        
        /// <remarks/>
        Bet,
        
        /// <remarks/>
        Raise,
        
        /// <remarks/>
        Returned,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Round {
        
        private Card[] communityCardsField;
        
        private Action[] actionsField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CommunityCards")]
        public Card[] CommunityCards {
            get {
                return this.communityCardsField;
            }
            set {
                this.communityCardsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Actions")]
        public Action[] Actions {
            get {
                return this.actionsField;
            }
            set {
                this.actionsField = value;
            }
        }
    }
}
