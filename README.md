# MyoMusc

![](/Assets/Images/Logo.png "MyoMusc")

###Overview
Over a weekend-long hackathon at the University of Maryland (Bitcamp), I built a gesture-controlled gamified therapy system for muscle-inhibiting neurological conditions along with one other team member.

###Background
Involuntary, uncontrollable muscle movements are characteristic of numerous conditions, such as dystonic tremors, stroke, or Huntington's disease. Physical therapy has been shown to be a non-invasive treatment that can help prevent the progression of and alleviate the symptoms of such conditions.

###Technologies
We used Unity and the Myo Armband, along with the growing concept of gamification, to produce therapy that promotes smooth, controlled arm movement. We detected "jitter" based on an algorithm that parsed the digital signal of the Myo's gyroscopic sensors, and increased the difficulty of the game upon greater turbulence. This posed a natural reinforcement to stabilize and improve.

###Reflections
This was my first time using both Unity and the Myo Armband. Integrating the APIs was fairly simple, but performing signal processing on the stream of data from the Myo sensors was quite difficult, and it required a large amount of fine-tuning in order to perfect game mechanics. I had a great time overall and learned a lot from mentors and other hackers!
