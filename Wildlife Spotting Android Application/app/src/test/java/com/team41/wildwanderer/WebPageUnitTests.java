package com.team41.wildwanderer;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.robolectric.RobolectricTestRunner;

import static org.junit.Assert.assertEquals;

/**
 * Tests for web page retrieval information
 * Author: Haico Maters
 */
@RunWith(RobolectricTestRunner.class)
public class WebPageUnitTests {

    WebpageInfo test = new WebpageInfo();

    //Expected tiger page gives the correct output info
    @Test
    public void getAnimalInfo(){
        String webPageOutput = test.wikipediaQuery("Tiger");
        assertEquals (webPageOutput, "Title: Tiger\n" +
                "Snippet: The (Panthera tigris) is the largest living cat species and a member of the genus Panthera. It is most recognisable for its dark vertical stripes\n" +
                "URL: https://en.wikipedia.org/wiki/Tiger\n" +
                "Photo URL: https://upload.wikimedia.org/wikipedia/commons/thumb/3/3f/Walking_tiger_female.jpg/300px-Walking_tiger_female.jpg");
    }

    //Expected only gives the url to the page for tigers on wikipedia
    @Test
    public void getURL(){
        assertEquals (test.extractURL("Title: Tiger\n" +
                "Snippet: The (Panthera tigris) is the largest living cat species and a member of the genus Panthera. It is most recognisable for its dark vertical stripes\n" +
                "URL: https://en.wikipedia.org/wiki/Tiger\n" +
                "Photo URL: https://upload.wikimedia.org/wikipedia/commons/thumb/3/3f/Walking_tiger_female.jpg/300px-Walking_tiger_female.jpg")
                , "https://en.wikipedia.org/wiki/Tiger");
    }

    // Expected class filtered to correct thing
    @Test
    public void filterClass(){
        assertEquals(test.filterClass("https://en.wikipedia.org/wiki/Tiger"), "Mammal-Like");
        assertEquals(test.filterClass("https://en.wikipedia.org/wiki/Owl"), "Bird-Like");
        assertEquals(test.filterClass("https://en.wikipedia.org/wiki/Ant"), "Bug-Like");
        assertEquals(test.filterClass("https://en.wikipedia.org/wiki/Shark") , "Fish-Like"); //Shark and goldfish are two different fish classes
        assertEquals(test.filterClass("https://en.wikipedia.org/wiki/Goldfish"), "Fish-Like");
        assertEquals(test.filterClass("https://en.wikipedia.org/wiki/Whale"), "Mammal-Like");
    }

}
