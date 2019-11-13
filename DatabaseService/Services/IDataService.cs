﻿using System.Collections.Generic;


namespace DatabaseService
{
    public interface IDataService
    {
        IList<Questions> GetQuestions(PagingAttributes pagingAttributes);
        int NumberOfQuestions();
        Questions GetQuestion(int questionId);
        //IList<Search> Search(string searchstring, int? searchtypecode, PagingAttributes pagingAttributes);
        IList<WordRank> WordRank(int userid, string searchstring, int searchtypecode, int? maxresults);
        //(Questions, IList<Answers>) GetThread(int questionId);
        IList<Posts> GetThread(int questionId);
        //void GetPostType(int postId);
        string GetPostType(int postId);
        //int GetParentId(int answerID);
        IList<Posts> Search(int userid, string searchstring, int? searchtypecode, PagingAttributes pagingAttributes);
        SinglePost GetPost(int postId);
    }
}
