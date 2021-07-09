# Error Handling

In F# error handling is normally handled with the Result monad or an exception. The choice of which to use when and were is largely left to the developer. While they both have there strengths I will like to talk about a specialized of working with the Result monad for a moment. If you have used it before then you realize it models a case where you have a success or ok path and an error path nicely (This is often referred to as ROP). We will leave this type at that and assume you understand the basics of ROP. (TODO: provide better background and resources but keep to a 2 min read)

## What is the problem?

When you are modeling errors there are two main goals I like to focus on:

1. Being able to match on a specific case.
1. Not having to map errors after each case.

Now how can we support both these ideas with minimal effort? Well there are a few ways I am sure, but I will offer up just one. 

## A solution

I tend to believe that errors are monoids