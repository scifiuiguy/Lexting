using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SpatialGestureText.LetterCube
{
    /// <summary>
    /// Allows user to test their typing speed
    /// </summary>
    public class SpeedTestManager : Singleton<SpeedTestManager>
    {
        #region vars
        [SerializeField] private float _testDuration;

        public InputField userOutput;
        public Text _testDisplay;
        public Text _confirmedDisplay;
        public Text _countdownDisplay;
        public Text wordsPerMinuteResult;
        public Text resultsConclusion;
        public ScrollRect testScroller;
        public RectTransform contentRectTransform;
        public CanvasGroup resultsCanvasGroup;

        /// <summary>
        /// 0 is bottom of scroll range, 1 is top
        /// </summary>
        [Range(0,1)]
        private float _targetScrollPosition = 1;
        private bool _isTestRunning;
        private IEnumerator _test, _scrollCheck;

        private string _confirmedOutput = "";

        private int _randomTestIndex;

        private int _currentLine = 1;

        private string[] _testContent = new string[] {
            "Word processors evolved dramatically once they became software programs rather than dedicated machines. They can usefully be distinguished from text editors, the category of software they evolved from. Word processing added to the text editor the ability to control type style and size, to manage lines (word wrap), to format documents into pages, and to number pages. Functions now taken for granted were added incrementally, sometimes by purchase of independent providers of add-on programs. Spell checking, grammar checking and mail merge were some of the most popular add-ons for early word processors. Word processors are also capable of hyphenation, and the management and correct positioning of footnotes and endnotes. Later desktop publishing programs were specifically designed with elaborate pre-formatted layouts for publication, offering only limited options for changing the layout, while allowing users to import text that was written using a text editor or word processor, or type the text in themselves.",
            "Two members of the 1984 class of Jefferson High School are chairing a group of 18 to look for a resort for the 20-year class reunion. A lovely place 78 miles from the city turns out to be the best.It has 254 rooms and a banquet hall to seat 378. It has been open 365 days per year since opening on May 30, 1926. They will need 450 to reserve the resort. Debbie Holmes was put in charge of buying 2,847 office machines for the entire firm.Debbie visited more than 109 companies in 35 states in 6 months.She will report to the board today in Room 2784 at 5 p.m.The board will consider her report about those 109 firms and recommend the top 2 or 3 brands to purchase.Debbie must decide before August 16. Lynn Greene said work started on the project March 27, 2003. The 246 blueprints were mailed to the office 18 days ago. The prints had to be 100 percent accurate before they were acceptable. The project should be finished by May 31, 2025. At that time there will be 47 new condominiums, each having at least 16 rooms.The building will be 25 stories.",
            "Income before securities transactions was up 10.8 percent from $13.49 million in 1982 to $14.95 million in 1983. Earnings per share (adjusted for a 10.5 percent stock dividend distributed on August 26) advanced 10 percent to $2.39 in 1983 from $2.17 in 1982. Earnings may rise for 7 years. Hopefully, earnings per share will grow another 10 percent. Kosy, Klemin, and Bille began selling on May 23, 1964. Their second store was founded in Renton on August 3, 1965. From 1964 to 1984, they opened more than 50 stores through-out the country. As they expanded, 12 regional offices had to be organized. Each of these 12 regional offices had to be organized. Each of these 12 regions employs from 108 to 578 people. National headquarters employs 1,077 people. Carole owns 118 stores located in 75 cities ranging as far west as Seattle and as far east as Boston. She owns 46 stores south of the Mason-Dixon line and 24 stores north of Denver. Carole buys goods from 89 companies located in 123 countries and all 50 states. Carole started in business on March 3, 1975. She had less than $6,000 in capital assets.",
            "The bikers rode down the long and narrow path to reach the city park. When they reached a good spot to rest, they began to look for signs of spring. The sun was bright, and a lot of bright red and blue blooms proved to all that warm spring days were the very best. Spring rides were planned. They had a burger at the lake and then rode farther up the mountain. As one rider started to get off his bike, he slipped and fell. One of the other bikers saw him fall but could do nothing to help him. Neither the boy nor the bike got hurt. After a brief stop, everyone was ready to go on. All the bikers enjoyed the nice view when they came to the top. All the roads far below them looked like ribbons. A dozen or so boats could be seen on the lake. It was very quiet and peaceful and no one wished to leave. As they set out on their return, they all enjoyed the ease of pedaling. The bikers came upon a new bike trail. This route led to scenery far grander than that seen from the normal path. The end of the day brought laughs and cheers from everyone. The fact that each person was very, very tired did not keep anyone from eagerly planning for the exciting ride to come.",
            "Jim and Anne will be in charge of the spring field day to be held in early June. They will ask their friends' aid to get set up. There will be games for the boys and girls. The children will want to hike, ride their bikes, and swim. This yearly event will be held in the new Peach Grove Park. Ruth has work to do on the plans for food for the day. Last year Ruth spent most of her time helping the two cooks with many snacks. Hot dogs, fries, soft drinks, ice cream, and candy apples were big sellers. Apple pie and ice cream sold well too. I hope Ruth serves the same food this year. George Long will hire the band and singer for the day. A great jazz band will play. George's mom leads the group. The jazz band is sure to be one of the big hits. George is to have them play from one to four and also in the evening. The fine songs they will play are sure to please all of us. Nice gifts will be given to all of the winners in each of the events. Local news coverage will include television and newspapers. Joyce Scott will take care of the pictures for the school paper and yearbook. Maybe the national news will do a short story on the tenth annual spring field day.",
            "Today, historians relate that, as a general rule, buying and selling securities was very much unorganized before the year 1792. Every person who owned a security faced the problem of finding interested buyers who might consider the purchase of a debt-free investment. This meant most people were somewhat slow in investing in stocks and bonds because these securities could not readily be converted into money. We have been told that an interesting number of traders and merchants agreed to try to do something to help correct the situation. At this first crucial meeting, they decided that it was a good idea to visit regularly on a daily basis to buy and sell securities. The group of leaders, whose meeting place was under an old, tall cottonwood tree, found the needed time to plot the financial future of our nation. We know from reading the old records that the original team who met together long ago in May became the very first members of the New York Stock Exchange. The New York Stock Exchange is still operating. Other stock exchanges conduct business in many countries around the world. Thousands and thousands of stocks and bonds are bought and sold each day.",
            "Trying to make a wise, good choice when thinking about what kinds of careers might be best for you is a hard thing to do. Your future may very well depend on the ways you go about finding the best job openings for you in the world of work. Some people will feel that there is one and only one job in the world for them. Hard thinking and a lot of hard work will help them find the one job that is best for them. Jobs are there for those with skills and a good work ethic. Many new young artists in the upper New England states are famous around the world as leaders in new American art. These fine artists are very good in their chosen fields and are willing to share their many talents by teaching others. The students have had the chance to learn and use skills in oil painting, sketching with chalk, sculpting, and weaving. Learning to typewrite is a skill that will help all of us in our work today. The development of the computer will open doors for people with the keyboarding skills and will make typing a necessity. Managers, as well as secretaries, will need skill at the keyboard to input data and process words. Therefore, good keyboarding skills may be important to you.",
            "Self-confidence is a tricky subject for many people. For some, it's impossible to feel good about themselves without outside validation. When you're in a situation where the people in your life aren't helping you to feel better about yourself, this can become a problem in your day to day life. Most insecurity stems from feelings of not being attractive or feelings of loneliness. If your insecurity doesn't necessarily stem from a lack of interaction, but more a lack of feeling attractive, there are other options that will help you online. Sometimes the best way to put your insecurities to rest can simply be to get an honest opinion. There are multiple support groups online where you can share a picture of yourself with other members and they will give honest feedback on your appearance. In most cases, they will point out good qualities that you may have missed in yourself. But you can trust them to be honest and many members give very valuable style and posture advice to increase your attractiveness. These practical tips and unbiased opinions from supportive strangers will immediately help you feel better about yourself, and if the tips are implemented it will also improve your self-esteem in the long-run.",
            "An ever-growing number of complex and rigid rules plus hard-to-cope-with regulations are now being legislated from state to state. Key federal regulations were formulated by the FDA, FTC, and the CPSC. Each of these federal agencies serves a specific mission. One example: Laws sponsored by the Office of the Fair Debt Collection Practices prevent an agency from purposefully harassing clients in serious debt. The Fair Packaging and Labeling Act makes certain that protection from misleading packaging of goods is guaranteed to each buyer of goods carried in small shops as well as in large supermarkets. Products on the market must reveal the names of all ingredients on the label. Language must be in clear and precise terms that can be understood by everyone. This practice is very crucial for the lives of many people. It is prudent that we recall that the FDA specifically requires that all goods are pure, safe, and wholesome. The FDA states that all goods be produced under highly sanitary conditions. Drugs must be completely safe and must also be effective for their stated purpose. This policy applies to cosmetics that must be both safe and pure. Individuals are often totally unappreciative of the FDA's great dedication.",
            "The recent emergence of several competitive typing websites has allowed several fast typists on computer keyboards to emerge along with new records, though these are unverifiable for the most part. Two of the most notable online records that are considered genuine are 241.82 wpm on an English text on typingzone.com by Brazilian Guilherme Sandrini (equivalent to 290.184 wpm using the traditional definition for words per minute since this site defines a word as six characters rather than five) and 256 wpm (a record caught on video) on TypeRacer by American Sean Wrona, the inaugural Ultimate Typing Championship winner, which was considered the highest ever legitimate score ever set on the site, until Wrona claimed it has been surpassed. Both of these records are essentially sprint speeds on short text selections lasting much less than one minute and were achieved on the QWERTY keyboard. Wrona also maintained 174 wpm on a 50-minute test taken on hi-games.net, another online typing website to unofficially displace Blackburn as the fastest endurance typist, although disputes might still arise over differences in the difficulty of the texts as well as Wrona's use of a modern computer keyboard as opposed to the typewriter used by Blackburn.",
            "One morning my friend and I were thinking about how we could plan our summer break away from school. Driving from our own state to several nearby states would help to expand our limited funds. Inviting six other friends to accompany us would lower our car expenses. Stopping at certain sites would also help us stretch our truly limited travel budget. Yesterday I engaged in an interesting and enlightening discussion about finances. I found it difficult to imagine that during my lifetime I might well earn at least one-half million dollars. It is also possible that I might spend as much as one-half million during the same period. The really difficult thing for me to do will be to save more of the half-million than I spend. Thinking about today's high cost of living makes this seem an impossible task for most. Last week I asked a friend to talk with me and a girl-friend about college. Our friend is the Dean of Women at a nearby college. The Dean and her staff spend much of their time talking to students who plan to go to college. The first thing she said was to work very hard each day in high school. Good grades are most important for being accepted. Being on time for classes and having a good view toward all phases of the school life are two other things to remember.",
            "Being human makes us susceptible to the onset of feelings. The role of these emotions varies. Some of them are useful while others may be harmful. The use of social media for self-expression has reached a point that it makes us feel we can say anything. This begins when we see people expressing anything and everything that come to mind. When we see everyone else voicing their likes and dislikes, their irritations and desires we tend to imitate what they do. And because many engage in this, we think that it is normal and healthy. However, when we get used to unbridled self-expression, we come to believe that all feelings are valid. We become convinced that in real life, we should also act on our emotions and our impulses. Using social media this way erodes our ability to regulate our actions and reactions. To illustrate, when something small irritates us we think that it's okay to feel this way. But isn't it better to foster one's patience and resilience instead of immediately complaining? Or when we develop an attraction to someone despite that person being in a relationship, and because social media has conditioned us that all feelings can be expressed, we tend to think that acting on this attraction is okay. Not all feelings deserve expression. We find ourselves creating our own problems when we let our present emotions control our actions.",
            "The Master of Business Administration (MBA or M.B.A.) degree originated in the United States in the early 20th century when the country industrialized and companies sought scientific approaches to management. The core courses in an MBA program cover various areas of business such as accounting, applied statistics, business communication, business ethics, business law, finance, managerial economics, management, entrepreneurship, marketing and operations in a manner most relevant to management analysis and strategy. Most programs also include elective courses and concentrations for further study in a particular area, for example accounting, finance, and marketing. MBA programs in the United States typically require completing about sixty credits, nearly twice the number of credits typically required for degrees that cover some of the same material such as the Master of Economics, Master of Finance, Master of Accountancy, Master of Science in Marketing and Master of Science in Management, The MBA is a terminal degree and a professional degree. Accreditation bodies specifically for MBA programs ensure consistency and quality of education. Business schools in many countries offer programs tailored to full-time, part-time, executive (abridged coursework typically occurring on nights or weekends) and distance learning students, many with specialized concentrations."
        };
        #endregion vars

        #region init
        private void Awake()
        {
            // currently all three of these are needed to run a successful test
            //RunTest();
            //StartCoroutine(InitScrollPos());
            //StartCoroutine(UnitTest_SimulateCompareCallback());
        }

        private IEnumerator InitScrollPos()
        {
            yield return new WaitForEndOfFrame();
            Tween.Value(testScroller.verticalScrollbar.value, 1,
                (value) =>
                {
                    testScroller.verticalScrollbar.value = value;
                }, 0.1f, 0
            );
        }
        #endregion init

        #region helper methods
        /// <summary>
        /// While test is running, determine if user output matches test display
        /// </summary>
        public void Compare()
        {
            if (!_isTestRunning) return;

            _confirmedOutput = "";
            var outputArray = userOutput.text.ToCharArray();
            var displayArray = _testDisplay.text.ToCharArray();

            for (int i = 0; i < outputArray.Length; i++)
                if (outputArray[i] == displayArray[i])
                    _confirmedOutput += outputArray[i];

            _confirmedDisplay.text = _confirmedOutput;
        }
        
        public void RunTest()
        {
            StopTest();
            _isTestRunning = true;
            _randomTestIndex = Random.Range(0, _testContent.Length - 1);
            _testDisplay.text = _testContent[_randomTestIndex];
            
            _test = Test();
            StartCoroutine(_test);
        }

        public void StopTest()
        {
            if (_test != null)
                StopCoroutine(_test);
            _isTestRunning = false;
        }

        private IEnumerator Test()
        {
            yield return new WaitForEndOfFrame();
            contentRectTransform.sizeDelta = new Vector2(
                contentRectTransform.sizeDelta.x,
                _testDisplay.cachedTextGenerator.lines[0].height * _testDisplay.cachedTextGenerator.lines.Count + 300f);
            _scrollCheck = CheckForScroll();
            StartCoroutine(_scrollCheck);
            yield return new WaitForSeconds(_testDuration);
            RenderResults();
            StopCoroutine(_scrollCheck);
            _isTestRunning = false;
        }

        private IEnumerator CheckForScroll()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                //_currentLine = TestDisplayCurrentLineLength();
                if (_confirmedDisplay.cachedTextGenerator.lines.Count != _currentLine)
                {
                    if (_currentLine < _confirmedDisplay.cachedTextGenerator.lines.Count)
                    {
                        _currentLine = _confirmedDisplay.cachedTextGenerator.lines.Count;
                        SetScrollPos(true);
                    }
                    else if (_currentLine > _confirmedDisplay.cachedTextGenerator.lines.Count)
                    {
                        _currentLine = _confirmedDisplay.cachedTextGenerator.lines.Count;
                        SetScrollPos(false);
                    }
                }
            }
        }

        private int TestDisplayCurrentLineLength()
        {
            Canvas.ForceUpdateCanvases();
            if (_confirmedDisplay.cachedTextGenerator.lines.Count < 2) return _confirmedDisplay.text.Length;
            else if (_confirmedDisplay.cachedTextGenerator.lines.Count >= _currentLine + 1)
                return _confirmedDisplay.cachedTextGenerator.lines[_currentLine+1].startCharIdx - 1;
            else return _confirmedDisplay.text.Length - _confirmedDisplay.cachedTextGenerator.lines[_currentLine].startCharIdx;
        }

        //private void AdvanceOneLine(bool directionChoice)
        //{
        //    if (_currentLine >= _testDisplay.cachedTextGenerator.lines.Count) return;
        //    //_currentLine++;
        //    SetScrollPos(_currentLine);
        //}

        /// <summary>
        /// Increments scroll position of the test display
        /// </summary>
        private void SetScrollPos(bool directionChoice)
        {
            float factor = 1f;

            int direction = (directionChoice) ? -1 : 1;

            if (_currentLine == 1)
                _targetScrollPosition = 1;
            else if (_testDisplay.cachedTextGenerator.lines.Count > _currentLine)
                _targetScrollPosition = _targetScrollPosition + direction * factor * _testDisplay.cachedTextGenerator.lines[_currentLine].height / (contentRectTransform.sizeDelta.y - 300);
            Tween.Value(testScroller.verticalScrollbar.value, _targetScrollPosition,
                (value) =>
                {
                    testScroller.verticalScrollbar.value = value;
                }, 0.1f, 0
            );
            Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Overload of SetScrollPos that iterates with a counter that's internal to this method for testing
        /// </summary>
        //private void SetScrollPos(bool directionChoice)
        //{
        //    float factor = 1f;

        //    if (count == 0)
        //        _targetScrollPosition = 1;
        //    else if (_testDisplay.cachedTextGenerator.lines.Count > count)
        //        _targetScrollPosition -= factor * _testDisplay.cachedTextGenerator.lines[count].height / (contentRectTransform.sizeDelta.y - 300);
        //    Tween.Value(testScroller.verticalScrollbar.value, _targetScrollPosition,
        //        (value) =>
        //        {
        //            testScroller.verticalScrollbar.value = value;
        //        }, 0.1f, 0
        //    );
        //    Canvas.ForceUpdateCanvases();
        //}

        private int CalculateNumberOfWordsComplete()
        {
            string[] confirmedArray = _confirmedOutput.Split(' ');
            return confirmedArray.Length;
        }

        private void RenderResults()
        {
            wordsPerMinuteResult.text = (CalculateNumberOfWordsComplete() * 60f / _testDuration).ToString(); ;
            resultsConclusion.text = RefreshConclusionText();
            Tween.CanvasGroupAlpha(resultsCanvasGroup, 1, 0.5f, 0);
        }

        private string RefreshConclusionText()
        {
            int complete = CalculateNumberOfWordsComplete();
            if (complete < 20)
                return "Keep practicing!";
            else if (complete < 40)
                return "You're getting there...";
            else if (complete < 60)
                return "Not bad.";
            else if (complete < 80)
                return "Nice job!";
            else if (complete < 100)
                return "Wow, you're quick!";
            else return "Unstoppable!!!";
        }
        #endregion helper methods

        #region Unit Tests
        // deprecated
        //private IEnumerator UnitTest_SetScrollPosSequentially()
        //{
        //    yield return new WaitForEndOfFrame();
        //    int count = 1;
        //    SetScrollPos(0);
        //    while (count < _testDisplay.cachedTextGenerator.lines.Count)
        //    {
        //        yield return new WaitForSeconds(2);
        //        SetScrollPos(count);
        //    }
        //    yield break;
        //}

        private IEnumerator UnitTest_SimulateCompareCallback()
        {
            int currentInputLength = 0;
            while (_isTestRunning)
            {
                yield return new WaitForSeconds(0.1f);
                if (userOutput.text.Length != currentInputLength)
                {
                    Compare();
                    currentInputLength = userOutput.text.Length;
                }
            }
        }
        #endregion Unit Tests
    }

}
