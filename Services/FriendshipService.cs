using Void.DTOs;
using Void.Models;
using Void.Repositories;

namespace Void.Services
{
    public class FriendshipService
    {
        private readonly FriendshipRepository _friendshipRepository;
        private readonly UserService _userService;

        public FriendshipService(FriendshipRepository friendshipRepository, UserService userService)
        {
            _friendshipRepository = friendshipRepository;
            _userService = userService;
        }

        public async Task<Friendship> SendFriendRequest(int userId, int friendId)
        {
            if (userId == friendId)
                throw new ArgumentException("Cannot send friend request to yourself");

            var existingFriendship = await _friendshipRepository.GetByUsersAsync(userId, friendId);
            if (existingFriendship != null)
            {
                if (existingFriendship.Status == FriendshipStatus.Pending)
                    throw new InvalidOperationException("Friend request already pending");

                if (existingFriendship.Status == FriendshipStatus.Accepted)
                    throw new InvalidOperationException("Already friends");

                if (existingFriendship.Status == FriendshipStatus.Blocked)
                    throw new InvalidOperationException("Cannot send request to blocked user");
            }

            var friend = await _userService.GetByIdAsync(friendId); if (friend == null)
                throw new ArgumentException("User not found");

            var friendship = new Friendship
            {
                UserId = userId,
                FriendId = friendId,
                Status = FriendshipStatus.Pending
            };

            await _friendshipRepository.AddAsync(friendship);
            return friendship;
        }

        public async Task<Friendship> RespondToFriendRequest(int friendshipId, int userId, bool accept)
        {
            var friendship = await _friendshipRepository.GetByIdAsync(friendshipId);

            if (friendship == null || friendship.FriendId != userId)
                throw new ArgumentException("Friend request not found");

            if (friendship.Status != FriendshipStatus.Pending)
                throw new InvalidOperationException("Friend request already processed");

            friendship.Status = accept ? FriendshipStatus.Accepted : FriendshipStatus.Rejected;

            await _friendshipRepository.UpdateAsync(friendship);
            return friendship;
        }

        public async Task<List<FriendshipDTO>> GetFriends(int userId)
        {
            var friendships = await _friendshipRepository.GetUserFriendshipsAsync(userId);

            return friendships.Select(f => new FriendshipDTO
            {
                Id = f.Id,
                UserId = f.UserId,
                FriendId = f.FriendId,
                FriendUsername = f.Friend.UserName,
                Status = f.Status,
                CreatedAt = f.CreatedAt
            }).ToList();
        }

        public async Task<List<FriendshipDTO>> GetPendingRequests(int userId)
        {
            var requests = await _friendshipRepository.GetPendingRequestsAsync(userId);

            return requests.Select(f => new FriendshipDTO
            {
                Id = f.Id,
                UserId = f.UserId,
                FriendId = f.FriendId,
                FriendUsername = f.User.UserName,
                Status = f.Status,
                CreatedAt = f.CreatedAt
            }).ToList();
        }

        public async Task<bool> RemoveFriend(int userId, int friendId)
        {
            var friendship = await _friendshipRepository.GetByUsersAsync(userId, friendId);

            if (friendship == null || friendship.Status != FriendshipStatus.Accepted)
                return false;

            return await _friendshipRepository.DeleteAsync(friendship.Id);
        }

        public async Task<bool> BlockUser(int userId, int blockUserId)
        {
            if (userId == blockUserId)
                throw new ArgumentException("Cannot block yourself");

            var friendship = await _friendshipRepository.GetByUsersAsync(userId, blockUserId);

            if (friendship == null)
            {
                friendship = new Friendship
                {
                    UserId = userId,
                    FriendId = blockUserId,
                    Status = FriendshipStatus.Blocked
                };
                await _friendshipRepository.AddAsync(friendship);
            }
            else
            {
                friendship.Status = FriendshipStatus.Blocked;
                await _friendshipRepository.UpdateAsync(friendship);
            }

            return true;
        }

        public async Task<bool> AreFriends(int userId, int friendId)
        {
            return await _friendshipRepository.AreFriends(userId, friendId);
        }
    }
}
