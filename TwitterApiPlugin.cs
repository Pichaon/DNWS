using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DNWS
{
    class TwitterApiPlugin : TwitterPlugin
    {
        public List<User> GetUsers()
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> users = context.Users.Where(b => true).Include(b => b.Following).ToList();
                    return users;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public List<Following> GetFollowing(string name)
        {
            using(var context = new TweetContext())
            {
                try
                {
                    List<User> follow= context.Users.Where(b => b.Name.Equals(name)).Include(b => b.Following).ToList();
                    return follow[0].Following;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override HTTPResponse GetResponse(HTTPRequest request)
        {
            HTTPResponse response = new HTTPResponse(200);
            string user = request.getRequestByKey("user");
            string password = request.getRequestByKey("password");
            string following = request.getRequestByKey("follow");
            string message = request.getRequestByKey("message");
            string[] link = request.Filename.Split("?");
            if (link[0] == "user")
            {
                if (request.Method == "GET")
                {
                    string json = JsonConvert.SerializeObject(GetUsers());
                    response.body = Encoding.UTF8.GetBytes(json);
                }
                else if (request.Method == "POST") 
                {//create new user require new username and newpassword
                    if(user!=null&&password!=null)
                    {
                        Twitter.AddUser(user, password);   
                        response.body = Encoding.UTF8.GetBytes("user added");
                    }

                }
                else if (request.Method == "DELETE")
                {   //delete user require username and password
                    Twitter twitter = new Twitter(user);
                    if(user != null && password != null)
                    {
                        twitter.DeleteUser(user);
                        response.body = Encoding.UTF8.GetBytes("deleted");
                    }                   
                }
            }
            else if (link[0] == "follow")
            {
                Twitter twitter = new Twitter(user);
                if (request.Method == "GET")
                {
                    string json = JsonConvert.SerializeObject(GetFollowing(user));
                    response.body = Encoding.UTF8.GetBytes(json);
                }
                else if (request.Method == "POST")
                {
                    if (Twitter.CheckUser(following))
                    {
                        twitter.AddFollowing(following);
                    }
                    else
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not exists");
                    }
                }
                else if (request.Method == "DELETE")
                {
                    try
                    {
                        twitter.RemoveFollowing(following);
                        response.body = Encoding.UTF8.GetBytes("deleted");
                    }
                    catch
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not exists");
                    }
                }
            }
            else if (link[0] == "tweet")
            {
                Twitter twitter = new Twitter(user);
                if (request.Method == "GET")
                {
                    try
                    {
                        string timeline = request.getRequestByKey("timeline");
                        if (timeline == "follow")
                        {
                            string json = JsonConvert.SerializeObject(twitter.GetFollowingTimeline());
                            response.body = Encoding.UTF8.GetBytes(json);
                        }
                        else
                        {
                            string json = JsonConvert.SerializeObject(twitter.GetUserTimeline());
                            response.body = Encoding.UTF8.GetBytes(json);
                        }
                    }
                    catch
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not found");
                    }
                }
                else if (request.Method == "POST")
                {
                    try
                    {
                        twitter.PostTweet(message);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                   catch
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not found");
                    }
                }
            }
            response.type = "application/json";
            return response;
        
        }
    }
}

